using Deullam.Credit.Inquiry.Challenge.Application.Messaging;

namespace Deullam.Credit.Inquiry.Challenge.Integration.Tests.Setup
{
    /// <summary>
    /// Uma mensagem que a API entregou ao <see cref="IMessagePublisher"/>.
    /// </summary>
    public sealed record PublishedMessage(string Topic, string Value);

    /// <summary>
    /// Dublê de teste do produtor Kafka.
    ///
    /// A API depende da abstração <see cref="IMessagePublisher"/> (implementada em produção por
    /// <c>KafkaPublisher</c>, na camada de infraestrutura). Nos testes de integração essa abstração
    /// é trocada por esta implementação, que apenas guarda o que seria publicado. Isso permite
    /// verificar o contrato de mensageria - tópico e payload - sem depender de um broker real.
    /// </summary>
    public sealed class FakeMessagePublisher : IMessagePublisher
    {
        private readonly List<PublishedMessage> _messages = new();
        private readonly object _sync = new();

        public IReadOnlyList<PublishedMessage> Messages
        {
            get
            {
                lock (_sync)
                {
                    return _messages.ToArray();
                }
            }
        }

        public Task PublishAsync(string topic, string message)
        {
            lock (_sync)
            {
                _messages.Add(new PublishedMessage(topic, message));
            }

            return Task.CompletedTask;
        }

        public void Clear()
        {
            lock (_sync)
            {
                _messages.Clear();
            }
        }
    }
}
