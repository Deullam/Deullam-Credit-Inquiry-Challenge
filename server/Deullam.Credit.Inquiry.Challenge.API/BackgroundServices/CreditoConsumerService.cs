using Confluent.Kafka;
using Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito;
using System.Text.Json;

namespace Deullam.Credit.Inquiry.Challenge.API.BackgroundServices
{
    public class CreditoConsumerService : BackgroundService
    {
        private readonly ILogger<CreditoConsumerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConsumer<Ignore, string> _consumer;
        private const string TopicName = "integrar-credito-constituido-entry";

        public CreditoConsumerService(ILogger<CreditoConsumerService> logger, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = configuration.GetConnectionString("Kafka"),
                GroupId = "credito-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(TopicName);
            _logger.LogInformation("Kafka Consumer iniciado. Aguardando por mensagens no tópico '{TopicName}'...", TopicName);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // O Consume espera por uma mensagem. O timeout de 500ms atende ao requisito.
                    var consumeResult = _consumer.Consume(TimeSpan.FromMilliseconds(500));

                    if (consumeResult is null) // Nenhuma mensagem recebida no timeout
                    {
                        await Task.Delay(100, stoppingToken); // Pequena pausa para não sobrecarregar a CPU
                        continue;
                    }

                    _logger.LogInformation("Mensagem recebida do Kafka: {Message}", consumeResult.Message.Value);

                    var creditoDto = JsonSerializer.Deserialize<CreditoDto>(consumeResult.Message.Value);

                    if (creditoDto != null)
                    {
                        // Criamos um escopo para resolver o ICreditoService, pois o BackgroundService é Singleton.
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var creditoService = scope.ServiceProvider.GetRequiredService<ICreditoService>();
                            await creditoService.CreateIfNotExistsAsync(creditoDto);
                            _logger.LogInformation("Crédito '{NumeroCredito}' processado com sucesso.", creditoDto.NumeroCredito);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao consumir ou processar mensagem do Kafka.");
                    // Em um cenário real, aqui teríamos uma política de retentativa ou envio para uma "dead-letter queue".
                }
            }
        }

        public override void Dispose()
        {
            _consumer.Close();
            _consumer.Dispose();
            base.Dispose();
        }
    }
}
