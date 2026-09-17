using System.Net;
using Deullam.Credit.Inquiry.Challenge.E2E.Tests.Setup;
using FluentAssertions;

namespace Deullam.Credit.Inquiry.Challenge.E2E.Tests.Features.GerenciarCredito
{
    /// <summary>
    /// CI-03 — Consultas a crédito e a NFS-e inexistentes têm comportamentos documentados e
    /// distintos: 404 por número de crédito, 200 com lista vazia por NFS-e. Só GETs contra o
    /// PostgreSQL real; Kafka não participa.
    /// </summary>
    [TestFixture]
    [Category("Core")]
    [Category("CI-03")]
    public class Ci03UnknownIdentifiersTests
    {
        private CreditApiClient _api = null!;

        /// <summary>Cria o cliente da API sobre o HttpClient compartilhado da suíte.</summary>
        [OneTimeSetUp]
        public void CreateApiClient() => _api = new CreditApiClient(RealStackFixture.Http);

        /// <summary>CI-03/AC1: numeroCredito que nunca foi integrado responde 404 (nunca 200 nem 500).</summary>
        [Test]
        [Description("CI-03: GET por numeroCredito que nunca existiu responde 404 Not Found")]
        public async Task shouldReturn404WhenNumeroCreditoNeverIntegrated()
        {
            var neverIntegrated = UniqueIds.NewNumeroCredito();

            using var response = await _api.GetByNumeroCreditoAsync(neverIntegrated);
            var body = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.NotFound, $"corpo da resposta: {body}");
            body.Should().Contain(neverIntegrated, "a mensagem de erro do middleware cita o número consultado");
        }

        /// <summary>
        /// CI-03/AC2: numeroNfse sem crédito associado responde 200 com lista vazia ([]), nunca 404 —
        /// contrato documentado no README e diferente do endpoint por número de crédito.
        /// </summary>
        [Test]
        [Description("CI-03: GET por numeroNfse sem créditos responde 200 com lista vazia, nunca 404")]
        public async Task shouldReturn200WithEmptyListWhenNumeroNfseHasNoCredits()
        {
            var neverUsedNfse = UniqueIds.NewNumeroNfse();

            using var response = await _api.GetByNfseAsync(neverUsedNfse);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var credits = await CreditApiClient.ReadCreditListAsync(response);
            credits.Should().BeEmpty();
        }
    }
}
