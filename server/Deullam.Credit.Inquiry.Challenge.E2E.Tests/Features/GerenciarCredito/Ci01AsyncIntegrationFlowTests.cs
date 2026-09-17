using System.Net;
using Deullam.Credit.Inquiry.Challenge.E2E.Tests.Setup;
using FluentAssertions;

namespace Deullam.Credit.Inquiry.Challenge.E2E.Tests.Features.GerenciarCredito
{
    /// <summary>
    /// CI-01 — Fluxo assíncrono completo ponta a ponta: POST real → Kafka real → consumidor real →
    /// PostgreSQL real → GET. Nada aqui é dublê: a mensagem atravessa o broker de verdade e quem
    /// grava é o <c>CreditoConsumerService</c> rodando dentro da API do compose.
    /// </summary>
    [TestFixture]
    [Category("Core")]
    [Category("CI-01")]
    public class Ci01AsyncIntegrationFlowTests
    {
        private CreditApiClient _api = null!;

        /// <summary>Cria o cliente da API sobre o HttpClient compartilhado da suíte.</summary>
        [OneTimeSetUp]
        public void CreateApiClient() => _api = new CreditApiClient(RealStackFixture.Http);

        /// <summary>CI-01/AC1: POST válido responde 202 com corpo confirmando o aceite.</summary>
        [Test]
        [Description("CI-01: POST com lista válida responde 202 Accepted com {\"success\": true}")]
        public async Task shouldReturn202AcceptedWhenPostingValidCredit()
        {
            var credit = CreditBuilder.AUniqueCredit().Build();

            using var response = await _api.PostCreditsAsync(new[] { credit });

            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().Be("{\"success\":true}");

            // Deixa o crédito ser processado antes do próximo teste, para não sobrar mensagem no tópico.
            await _api.WaitUntilCreditIsQueryableAsync(credit.NumeroCredito);
        }

        /// <summary>
        /// CI-01/AC2 (negativo): logo após o 202 o GET ainda é 404 — o 202 é aceite de enfileiramento,
        /// não confirmação de gravação. Um 200 aqui com um crédito DIFERENTE do enviado é vazamento de
        /// estado (numeroCredito já existia); um 200 com o mesmo crédito é o consumidor vencendo a
        /// corrida com o GET imediato. Os dois falham, com diagnóstico distinto.
        /// </summary>
        [Test]
        [Description("CI-01: GET imediatamente após o 202 responde 404 (o 202 não confirma gravação)")]
        public async Task shouldReturn404ImmediatelyAfter202BeforeConsumerProcesses()
        {
            var credit = CreditBuilder.AUniqueCredit().Build();

            using var accepted = await _api.PostCreditsAsync(new[] { credit });
            accepted.StatusCode.Should().Be(HttpStatusCode.Accepted);

            using var immediate = await _api.GetByNumeroCreditoAsync(credit.NumeroCredito);

            if (immediate.StatusCode == HttpStatusCode.OK)
            {
                var found = await CreditApiClient.ReadCreditAsync(immediate);
                var sameCredit = found.NumeroNfse == credit.NumeroNfse && found.ValorIssqn == credit.ValorIssqn;
                Assert.Fail(sameCredit
                    ? $"GET imediato após o 202 respondeu 200 com o próprio crédito {credit.NumeroCredito}: o consumidor " +
                      "processou antes do GET imediato — a asserção '404 logo após o 202' perdeu a corrida com o consumidor real."
                    : $"GET imediato após o 202 respondeu 200 com um crédito DIFERENTE do enviado: o numeroCredito " +
                      $"{credit.NumeroCredito} já existia no banco antes do POST — vazamento de estado entre execuções.");
            }

            immediate.StatusCode.Should().Be(
                HttpStatusCode.NotFound,
                "o 202 é um aceite de enfileiramento, não uma confirmação de gravação");

            await _api.WaitUntilCreditIsQueryableAsync(credit.NumeroCredito);
        }

        /// <summary>
        /// CI-01/AC3: depois que o consumidor real processa a mensagem, o GET responde 200 com todos
        /// os campos enviados, inclusive o SimplesNacional "Sim"/"Não" preservado pelo mapeamento
        /// string → bool → string.
        /// </summary>
        [Test]
        [Description("CI-01: após o consumidor real processar, GET por numeroCredito responde 200 com todos os campos enviados")]
        public async Task shouldReturn200WithAllFieldsAfterConsumerProcessesMessage()
        {
            var credit = CreditBuilder.AUniqueCredit().WithSimplesNacional("Não").Build();

            using var accepted = await _api.PostCreditsAsync(new[] { credit });
            accepted.StatusCode.Should().Be(HttpStatusCode.Accepted);

            var stored = await _api.WaitUntilCreditIsQueryableAsync(credit.NumeroCredito);

            stored.Should().BeEquivalentTo(credit);
            stored.SimplesNacional.Should().Be("Não");
        }

        /// <summary>CI-01/AC4: o crédito processado aparece na listagem por NFS-e.</summary>
        [Test]
        [Description("CI-01: após o processamento, GET por numeroNfse inclui o numeroCredito recém-criado")]
        public async Task shouldListNewCreditInNfseQueryAfterProcessing()
        {
            var credit = CreditBuilder.AUniqueCredit().WithSimplesNacional("Sim").Build();

            using var accepted = await _api.PostCreditsAsync(new[] { credit });
            accepted.StatusCode.Should().Be(HttpStatusCode.Accepted);
            await _api.WaitUntilCreditIsQueryableAsync(credit.NumeroCredito);

            using var byNfse = await _api.GetByNfseAsync(credit.NumeroNfse);

            byNfse.StatusCode.Should().Be(HttpStatusCode.OK);
            var credits = await CreditApiClient.ReadCreditListAsync(byNfse);
            credits.Should().ContainSingle(c => c.NumeroCredito == credit.NumeroCredito)
                .Which.Should().BeEquivalentTo(credit);
        }

        /// <summary>
        /// CI-01/AC5: uma lista com vários créditos gera UMA mensagem por item no tópico real (provado
        /// pelo probe Kafka) e cada numeroCredito fica consultável individualmente.
        /// </summary>
        [Test]
        [Description("CI-01: lista com mais de um crédito gera uma mensagem por item e cada um fica consultável")]
        public async Task shouldProcessEachItemOfAMultiCreditListIndependently()
        {
            var sharedNfse = UniqueIds.NewNumeroNfse();
            var credits = new[]
            {
                CreditBuilder.AUniqueCredit().WithNumeroNfse(sharedNfse).WithValorIssqn(10.10m).Build(),
                CreditBuilder.AUniqueCredit().WithNumeroNfse(sharedNfse).WithValorIssqn(20.20m).Build(),
                CreditBuilder.AUniqueCredit().WithNumeroNfse(sharedNfse).WithValorIssqn(30.30m).Build()
            };

            using var probe = new KafkaTopicProbe();
            probe.CaptureBaseline();

            using var accepted = await _api.PostCreditsAsync(credits);
            accepted.StatusCode.Should().Be(HttpStatusCode.Accepted);

            var published = probe.CollectNewMessages(
                E2ESettings.ConsumerBudget,
                stopWhen: seen => seen.Count >= credits.Length);

            published.Should().HaveCount(credits.Length, "o POST publica exatamente uma mensagem por crédito");
            foreach (var credit in credits)
            {
                published.Should().ContainSingle(payload => payload.Contains(credit.NumeroCredito),
                    $"o crédito {credit.NumeroCredito} deveria ter a sua própria mensagem no tópico");
            }

            foreach (var credit in credits)
            {
                var stored = await _api.WaitUntilCreditIsQueryableAsync(credit.NumeroCredito);
                stored.Should().BeEquivalentTo(credit);
            }

            using var byNfse = await _api.GetByNfseAsync(sharedNfse);
            var listed = await CreditApiClient.ReadCreditListAsync(byNfse);
            listed.Select(c => c.NumeroCredito).Should().BeEquivalentTo(credits.Select(c => c.NumeroCredito));
        }
    }
}
