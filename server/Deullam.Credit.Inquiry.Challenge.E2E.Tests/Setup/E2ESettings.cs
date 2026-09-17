namespace Deullam.Credit.Inquiry.Challenge.E2E.Tests.Setup
{
    /// <summary>
    /// Configuração da suíte E2E. Tudo vem de variável de ambiente com um default que bate com o
    /// <c>server/docker-compose.yml</c>: a API publicada em <c>http://localhost:8080</c> e o Kafka
    /// no listener <c>PLAINTEXT_HOST</c> em <c>localhost:29092</c>. Os orçamentos de tempo ficam
    /// centralizados aqui para que nenhum teste invente o seu.
    /// </summary>
    public static class E2ESettings
    {
        /// <summary>Variável de ambiente com a URL base da API real.</summary>
        public const string ApiBaseUrlVariable = "E2E_API_BASE_URL";

        /// <summary>Variável de ambiente com o bootstrap do broker Kafka visto do host.</summary>
        public const string KafkaBootstrapVariable = "E2E_KAFKA_BOOTSTRAP";

        /// <summary>Tópico em que o POST publica e que o <c>CreditoConsumerService</c> consome.</summary>
        public const string InboundTopic = "integrar-credito-constituido-entry";

        /// <summary>Identificador desta execução, embutido em todo numeroCredito/numeroNfse gerado.</summary>
        public static readonly string RunId = Guid.NewGuid().ToString("N")[..8];

        /// <summary>Orçamento para a API responder 200 em /health/ready antes do primeiro teste.</summary>
        public static readonly TimeSpan ReadinessBudget = TimeSpan.FromSeconds(90);

        /// <summary>Intervalo entre tentativas de /health/ready durante a espera inicial.</summary>
        public static readonly TimeSpan ReadinessInterval = TimeSpan.FromSeconds(1);

        /// <summary>Orçamento para o consumidor real processar uma mensagem e o GET passar a responder 200.</summary>
        public static readonly TimeSpan ConsumerBudget = TimeSpan.FromSeconds(20);

        /// <summary>Intervalo entre GETs no polling que espera o consumidor.</summary>
        public static readonly TimeSpan PollingInterval = TimeSpan.FromMilliseconds(250);

        /// <summary>
        /// Janela curta em que o probe Kafka observa o tópico para provar AUSÊNCIA de mensagem nova
        /// depois de um POST inválido.
        /// </summary>
        public static readonly TimeSpan KafkaSilenceWindow = TimeSpan.FromSeconds(3);

        /// <summary>Timeout de cada chamada HTTP individual.</summary>
        public static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(30);

        /// <summary>URL base da API real (default <c>http://localhost:8080</c>).</summary>
        public static Uri ApiBaseUrl => new(Read(ApiBaseUrlVariable, "http://localhost:8080"));

        /// <summary>Bootstrap servers do Kafka real (default <c>localhost:29092</c>).</summary>
        public static string KafkaBootstrapServers => Read(KafkaBootstrapVariable, "localhost:29092");

        /// <summary>Lê a variável de ambiente ou devolve o default quando ela está ausente/vazia.</summary>
        private static string Read(string variable, string fallback)
        {
            var value = Environment.GetEnvironmentVariable(variable);
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}
