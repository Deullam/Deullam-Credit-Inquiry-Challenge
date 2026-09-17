using System.Diagnostics;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace Deullam.Credit.Inquiry.Challenge.E2E.Tests.Setup
{
    /// <summary>
    /// Consumidor Kafka de prova sobre o broker REAL, usado para afirmar o que a API publicou (ou
    /// não publicou) no tópico de entrada. Desenho:
    ///
    /// <list type="number">
    ///   <item>
    ///     <see cref="CaptureBaseline"/> é chamado ANTES do POST sob teste: consulta o high watermark
    ///     de cada partição do tópico e faz <c>Assign</c> exatamente nesses offsets. Tudo que existir
    ///     antes fica invisível; só mensagens produzidas depois da baseline são lidas.
    ///   </item>
    ///   <item>
    ///     <see cref="CollectNewMessages"/> lê durante uma janela curta e explícita
    ///     (<see cref="E2ESettings.KafkaSilenceWindow"/> para provar ausência) e devolve os payloads.
    ///   </item>
    ///   <item>
    ///     O group id é único por instância (<c>e2e-probe-{RunId}-{sufixo}</c>), auto-commit
    ///     desligado e nenhuma inscrição por grupo: o probe nunca disputa partições com o
    ///     <c>credito-consumer-group</c> da API nem deixa rastro de offset no broker.
    ///   </item>
    ///   <item>
    ///     Contra falso positivo por mensagens de OUTROS testes: a suíte roda em série
    ///     (<c>[assembly: Parallelizable(ParallelScope.None)]</c>), logo nenhum outro POST acontece
    ///     durante a janela; além disso, todo identificador gerado nesta execução leva o prefixo
    ///     <c>E2E-{RunId}</c>, e os payloads lidos vão inteiros na mensagem de falha, para que um
    ///     vazamento seja diagnosticável em vez de silencioso.
    ///   </item>
    ///   <item>
    ///     Tópico ainda inexistente (pilha recém-criada, nenhum POST válido antes): o probe cria o
    ///     tópico com 1 partição — o mesmo que o auto-create do broker faria no primeiro produce —
    ///     para que a baseline exista e a leitura seja determinística.
    ///   </item>
    /// </list>
    /// </summary>
    public sealed class KafkaTopicProbe : IDisposable
    {
        private static readonly TimeSpan BrokerTimeout = TimeSpan.FromSeconds(10);

        private readonly string _bootstrapServers;
        private readonly string _topic;
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly List<TopicPartitionOffset> _baseline = new();

        /// <summary>Cria o probe apontando para o broker e o tópico da suíte.</summary>
        public KafkaTopicProbe(string? bootstrapServers = null, string? topic = null)
        {
            _bootstrapServers = bootstrapServers ?? E2ESettings.KafkaBootstrapServers;
            _topic = topic ?? E2ESettings.InboundTopic;

            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = $"e2e-probe-{E2ESettings.RunId}-{Guid.NewGuid():N}"[..40],
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false,
                AutoOffsetReset = AutoOffsetReset.Latest,
                EnablePartitionEof = false,
                AllowAutoCreateTopics = false
            };

            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        }

        /// <summary>Offsets finais capturados na baseline (para log/diagnóstico).</summary>
        public IReadOnlyList<TopicPartitionOffset> Baseline => _baseline;

        /// <summary>
        /// Captura o high watermark de cada partição e posiciona o consumidor neles. Deve ser chamado
        /// ANTES do POST sob observação. Cria o tópico se ele ainda não existir.
        /// </summary>
        public void CaptureBaseline()
        {
            var partitions = EnsureTopicAndGetPartitions();

            _baseline.Clear();
            foreach (var partitionId in partitions)
            {
                var topicPartition = new TopicPartition(_topic, partitionId);
                var watermarks = _consumer.QueryWatermarkOffsets(topicPartition, BrokerTimeout);
                _baseline.Add(new TopicPartitionOffset(topicPartition, watermarks.High));
            }

            _consumer.Assign(_baseline);

            TestContext.Progress.WriteLine(
                $"[E2E] probe Kafka posicionado em: {string.Join(", ", _baseline.Select(b => $"{b.Partition.Value}@{b.Offset.Value}"))}");
        }

        /// <summary>
        /// Lê o tópico a partir da baseline durante <paramref name="window"/> e devolve os payloads
        /// novos. Com <paramref name="stopWhen"/>, encerra assim que o predicado for satisfeito (uso
        /// positivo: "já vi as N mensagens que esperava"); sem ele, consome a janela inteira (uso
        /// negativo: "nada chegou em 3s").
        /// </summary>
        public IReadOnlyList<string> CollectNewMessages(TimeSpan window, Func<IReadOnlyList<string>, bool>? stopWhen = null)
        {
            Assert.That(_baseline, Is.Not.Empty, "CaptureBaseline() precisa ser chamado antes de CollectNewMessages()");

            var collected = new List<string>();
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < window)
            {
                var remaining = window - stopwatch.Elapsed;
                var result = _consumer.Consume(remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero);

                if (result?.Message?.Value is null)
                {
                    continue;
                }

                collected.Add(result.Message.Value);

                if (stopWhen is not null && stopWhen(collected))
                {
                    break;
                }
            }

            return collected;
        }

        /// <summary>Encerra o consumidor sem commitar nada.</summary>
        public void Dispose()
        {
            _consumer.Close();
            _consumer.Dispose();
        }

        /// <summary>
        /// Garante que o tópico existe e devolve os ids das partições. Se o tópico ainda não existe,
        /// cria com 1 partição (o mesmo que o auto-create do broker faz no primeiro produce da API).
        /// </summary>
        private IReadOnlyList<int> EnsureTopicAndGetPartitions()
        {
            using var admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _bootstrapServers }).Build();

            var partitions = ReadPartitions(admin);
            if (partitions.Count > 0)
            {
                return partitions;
            }

            TestContext.Progress.WriteLine($"[E2E] tópico '{_topic}' ainda não existe; criando com 1 partição para a baseline.");
            CreateTopic(admin);

            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < BrokerTimeout)
            {
                partitions = ReadPartitions(admin);
                if (partitions.Count > 0)
                {
                    return partitions;
                }

                Thread.Sleep(200);
            }

            Assert.Fail($"O tópico '{_topic}' não apareceu nos metadados do broker {_bootstrapServers} em {BrokerTimeout.TotalSeconds:F0}s após a criação.");
            return Array.Empty<int>();
        }

        /// <summary>Partições conhecidas do tópico; lista vazia quando o broker ainda não o conhece.</summary>
        private List<int> ReadPartitions(IAdminClient admin)
        {
            var metadata = admin.GetMetadata(_topic, BrokerTimeout);
            var topicMetadata = metadata.Topics.SingleOrDefault(t => t.Topic == _topic);

            if (topicMetadata is null || topicMetadata.Error.IsError)
            {
                return new List<int>();
            }

            return topicMetadata.Partitions.Select(p => p.PartitionId).OrderBy(id => id).ToList();
        }

        /// <summary>
        /// Cria o tópico. A única falha tolerada é <see cref="ErrorCode.TopicAlreadyExists"/> (corrida
        /// com o auto-create disparado por um produce da API); qualquer outra sobe.
        /// </summary>
        private void CreateTopic(IAdminClient admin)
        {
            try
            {
                admin.CreateTopicsAsync(new[]
                {
                    new TopicSpecification { Name = _topic, NumPartitions = 1, ReplicationFactor = 1 }
                }).GetAwaiter().GetResult();
            }
            catch (CreateTopicsException ex) when (ex.Results.All(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
            {
                TestContext.Progress.WriteLine($"[E2E] tópico '{_topic}' foi criado por outro cliente no meio-tempo; seguindo.");
            }
        }
    }
}
