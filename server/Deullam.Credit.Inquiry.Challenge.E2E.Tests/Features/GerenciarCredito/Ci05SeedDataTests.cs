using System.Net;
using Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito;
using Deullam.Credit.Inquiry.Challenge.E2E.Tests.Setup;
using FluentAssertions;

namespace Deullam.Credit.Inquiry.Challenge.E2E.Tests.Features.GerenciarCredito
{
    /// <summary>
    /// CI-05 — A consulta reflete o seed aplicado pela migration <c>AddSeedData</c>
    /// (20251219003658) via <c>make migrate</c>. Os valores esperados abaixo são cópia literal da
    /// migration. Só é aplicável ao PostgreSQL real com as migrations do EF Core aplicadas — não
    /// vale para SQLite/EnsureCreated (fallback da suíte de integração), que não tem este seed.
    /// </summary>
    [TestFixture]
    [Category("Core")]
    [Category("CI-05")]
    public class Ci05SeedDataTests
    {
        private const string SeededNfse = "7891011";
        private const string MigrateHint = "rode `cd server && make migrate` para aplicar InitialCreate + AddSeedData";

        /// <summary>Linha 1 da migration AddSeedData (SimplesNacional true → "Sim").</summary>
        private static readonly CreditoDto SeededCredit123456 = new()
        {
            NumeroCredito = "123456",
            NumeroNfse = SeededNfse,
            DataConstituicao = new DateTime(2024, 2, 25),
            ValorIssqn = 1500.75m,
            TipoCredito = "ISSQN",
            SimplesNacional = "Sim",
            Aliquota = 5.0m,
            ValorFaturado = 30000.00m,
            ValorDeducao = 5000.00m,
            BaseCalculo = 25000.00m
        };

        /// <summary>Linha 2 da migration AddSeedData (SimplesNacional false → "Não").</summary>
        private static readonly CreditoDto SeededCredit789012 = new()
        {
            NumeroCredito = "789012",
            NumeroNfse = SeededNfse,
            DataConstituicao = new DateTime(2024, 2, 26),
            ValorIssqn = 1200.50m,
            TipoCredito = "ISSQN",
            SimplesNacional = "Não",
            Aliquota = 4.5m,
            ValorFaturado = 25000.00m,
            ValorDeducao = 4000.00m,
            BaseCalculo = 21000.00m
        };

        private CreditApiClient _api = null!;

        /// <summary>Cria o cliente da API sobre o HttpClient compartilhado da suíte.</summary>
        [OneTimeSetUp]
        public void CreateApiClient() => _api = new CreditApiClient(RealStackFixture.Http);

        /// <summary>CI-05/AC1: a NFS-e 7891011 lista ao menos os créditos 123456 e 789012, com os valores do seed.</summary>
        [Test]
        [Description("CI-05: GET /api/creditos/nfse/7891011 responde 200 com os créditos 123456 e 789012 do seed")]
        public async Task shouldListSeededCreditsWhenQueryingNfse7891011()
        {
            using var response = await _api.GetByNfseAsync(SeededNfse);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var credits = await CreditApiClient.ReadCreditListAsync(response);

            credits.Should().NotBeEmpty(MigrateHint);
            credits.Select(c => c.NumeroCredito).Should().Contain(new[] { "123456", "789012" }, MigrateHint);
            credits.Single(c => c.NumeroCredito == "123456").Should().BeEquivalentTo(SeededCredit123456);
            credits.Single(c => c.NumeroCredito == "789012").Should().BeEquivalentTo(SeededCredit789012);
        }

        /// <summary>CI-05/AC2: crédito 123456 com SimplesNacional "Sim" (mapeado de true) e os demais campos do seed.</summary>
        [Test]
        [Description("CI-05: GET /api/creditos/credito/123456 responde 200 com SimplesNacional=\"Sim\" e os campos do seed")]
        public async Task shouldReturnSimplesNacionalSimForCredit123456()
        {
            using var response = await _api.GetByNumeroCreditoAsync("123456");

            response.StatusCode.Should().Be(HttpStatusCode.OK, MigrateHint);
            var credit = await CreditApiClient.ReadCreditAsync(response);

            credit.SimplesNacional.Should().Be("Sim");
            credit.Should().BeEquivalentTo(SeededCredit123456);
        }

        /// <summary>CI-05/AC3: crédito 789012 com SimplesNacional "Não" (mapeado de false) e os demais campos do seed.</summary>
        [Test]
        [Description("CI-05: GET /api/creditos/credito/789012 responde 200 com SimplesNacional=\"Não\" e os campos do seed")]
        public async Task shouldReturnSimplesNacionalNaoForCredit789012()
        {
            using var response = await _api.GetByNumeroCreditoAsync("789012");

            response.StatusCode.Should().Be(HttpStatusCode.OK, MigrateHint);
            var credit = await CreditApiClient.ReadCreditAsync(response);

            credit.SimplesNacional.Should().Be("Não");
            credit.Should().BeEquivalentTo(SeededCredit789012);
        }
    }
}
