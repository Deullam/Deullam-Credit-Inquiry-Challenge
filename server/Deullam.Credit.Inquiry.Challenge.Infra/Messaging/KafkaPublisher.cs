using Confluent.Kafka;
using Deullam.Credit.Inquiry.Challenge.Application.Messaging;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Deullam.Credit.Inquiry.Challenge.Infra.Messaging
{
    public class KafkaPublisher : IMessagePublisher, IDisposable
    {
        private readonly IProducer<Null, string> _producer;
        private bool _disposed;

        public KafkaPublisher(IConfiguration configuration)
        {
            var bootstrap = configuration.GetConnectionString("Kafka")
                ?? throw new InvalidOperationException("Connection string 'Kafka' not found. Configure a connection string named 'Kafka'.");

            var config = new ProducerConfig
            {
                BootstrapServers = bootstrap,

                MessageTimeoutMs = 10000
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task PublishAsync(string topic, string message)
        {
            if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("topic é obrigatório", nameof(topic));
            if (message is null) throw new ArgumentNullException(nameof(message));

            try
            {
                // Produz a mensagem e espera pelo ack do broker
                await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message }).ConfigureAwait(false);
            }
            finally
            {
                // O Flush garante que todas as mensagens pendentes na fila do producer
                // sejam enviadas antes de continuar. Isso pode ajudar a evitar timeouts
                // em cenários de desligamento rápido, mas também é útil para depuração.
                _producer.Flush(TimeSpan.FromSeconds(10));
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _producer.Flush(TimeSpan.FromSeconds(5));
            _producer.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
