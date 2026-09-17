using System.Net;
using System.Text.Json;
using Deullam.Credit.Inquiry.Challenge.E2E.Tests.Setup;
using FluentAssertions;

namespace Deullam.Credit.Inquiry.Challenge.E2E.Tests.Features.Health
{
    /// <summary>
    /// CI-06 (caminho saudável apenas) — com PostgreSQL e Kafka reais no ar, <c>/health/ready</c>
    /// reporta as duas dependências Healthy e <c>/health/self</c> responde Healthy sem consultar
    /// nenhuma delas (entries vazio). Os cenários com dependência fora do ar (stop/start de
    /// contêineres) ficam FORA desta rodada, em item próprio marcado como invasivo.
    /// </summary>
    [TestFixture]
    [Category("Core")]
    [Category("CI-06")]
    public class Ci06HealthEndpointsTests
    {
        private CreditApiClient _api = null!;

        /// <summary>Cria o cliente da API sobre o HttpClient compartilhado da suíte.</summary>
        [OneTimeSetUp]
        public void CreateApiClient() => _api = new CreditApiClient(RealStackFixture.Http);

        /// <summary>CI-06/AC1: readiness 200 Healthy com entries.PostgreSQL e entries.Kafka Healthy.</summary>
        [Test]
        [Description("CI-06: com Postgres e Kafka no ar, GET /health/ready responde 200 Healthy com PostgreSQL e Kafka Healthy")]
        public async Task shouldReportPostgreSqlAndKafkaHealthyOnReadyWhenStackIsUp()
        {
            using var response = await _api.GetHealthAsync("/health/ready");
            var body = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK, $"corpo: {body}");

            using var report = JsonDocument.Parse(body);
            var root = report.RootElement;
            root.GetProperty("status").GetString().Should().Be("Healthy");

            var entries = root.GetProperty("entries");
            entries.EnumerateObject().Select(e => e.Name).Should().BeEquivalentTo(new[] { "PostgreSQL", "Kafka" });
            entries.GetProperty("PostgreSQL").GetProperty("status").GetString().Should().Be("Healthy");
            entries.GetProperty("Kafka").GetProperty("status").GetString().Should().Be("Healthy");
        }

        /// <summary>CI-06/AC2: liveness 200 Healthy com entries vazio — não consulta dependência alguma.</summary>
        [Test]
        [Description("CI-06: GET /health/self responde 200 Healthy com entries vazio")]
        public async Task shouldReportHealthyWithNoEntriesOnSelf()
        {
            using var response = await _api.GetHealthAsync("/health/self");
            var body = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK, $"corpo: {body}");

            using var report = JsonDocument.Parse(body);
            var root = report.RootElement;
            root.GetProperty("status").GetString().Should().Be("Healthy");
            root.GetProperty("entries").ValueKind.Should().Be(JsonValueKind.Object);
            root.GetProperty("entries").EnumerateObject().Should().BeEmpty("liveness não consulta PostgreSQL nem Kafka");
        }
    }
}
