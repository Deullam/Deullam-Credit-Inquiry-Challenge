using System.Net;
using Deullam.Credit.Inquiry.Challenge.E2E.Tests.Setup;
using FluentAssertions;

namespace Deullam.Credit.Inquiry.Challenge.E2E.Tests.Features.GerenciarCredito
{
    /// <summary>
    /// CI-02 — POST inválido é rejeitado na borda (400) e NENHUMA mensagem chega ao Kafka real. A
    /// ausência é provada pelo <see cref="KafkaTopicProbe"/>: baseline nos offsets finais do tópico
    /// capturada antes de cada POST e leitura durante uma janela curta e explícita
    /// (<see cref="E2ESettings.KafkaSilenceWindow"/>) depois dele. Qualquer payload que apareça vai
    /// inteiro na mensagem de falha.
    /// </summary>
    [TestFixture]
    [Category("Core")]
    [Category("CI-02")]
    public class Ci02InvalidPostPublishesNothingTests
    {
        private CreditApiClient _api = null!;

        /// <summary>Cria o cliente da API sobre o HttpClient compartilhado da suíte.</summary>
        [OneTimeSetUp]
        public void CreateApiClient() => _api = new CreditApiClient(RealStackFixture.Http);

        /// <summary>
        /// CI-02/AC1: corpo JSON <c>null</c>. O model binding do [ApiController] recusa o corpo nulo antes
        /// do guard do controller, com o ProblemDetails de validação ("errors").
        /// </summary>
        [Test]
        [Description("CI-02: lista nula responde 400 e nenhuma mensagem nova chega ao tópico real")]
        public async Task shouldReturn400AndPublishNothingWhenListIsNull()
        {
            await AssertRejectedWithoutPublishingAsync(
                () => _api.PostRawAsync("null"),
                expectedBodyFragment: "errors");
        }

        /// <summary>CI-02/AC1: lista vazia recusada pelo guard do controller com a mensagem documentada.</summary>
        [Test]
        [Description("CI-02: lista vazia responde 400 'nula ou vazia' e nenhuma mensagem nova chega ao tópico real")]
        public async Task shouldReturn400AndPublishNothingWhenListIsEmpty()
        {
            await AssertRejectedWithoutPublishingAsync(
                () => _api.PostRawAsync("[]"),
                expectedBodyFragment: "não pode ser nula ou vazia");
        }

        /// <summary>CI-02/AC2: campo obrigatório (numeroCredito) ausente → 400 com os erros do model binding.</summary>
        [Test]
        [Description("CI-02: campo obrigatório ausente responde 400 com erros de validação e nada é publicado")]
        public async Task shouldReturn400AndPublishNothingWhenRequiredFieldIsMissing()
        {
            var bodyWithoutNumeroCredito =
                $$"""
                [{ "numeroNfse": "{{UniqueIds.NewNumeroNfse()}}", "dataConstituicao": "2024-02-25", "valorIssqn": 100.50,
                   "tipoCredito": "ISSQN", "simplesNacional": "Sim", "aliquota": 5.0,
                   "valorFaturado": 2010.00, "valorDeducao": 0, "baseCalculo": 2010.00 }]
                """;

            await AssertRejectedWithoutPublishingAsync(
                () => _api.PostRawAsync(bodyWithoutNumeroCredito),
                expectedBodyFragment: "errors");
        }

        /// <summary>CI-02/AC2: tipo incompatível (valorIssqn como texto) → 400 com os erros do model binding.</summary>
        [Test]
        [Description("CI-02: tipo incompatível responde 400 com erros de validação e nada é publicado")]
        public async Task shouldReturn400AndPublishNothingWhenFieldTypeIsIncompatible()
        {
            var bodyWithTextInsteadOfDecimal =
                $$"""
                [{ "numeroCredito": "{{UniqueIds.NewNumeroCredito()}}", "numeroNfse": "{{UniqueIds.NewNumeroNfse()}}",
                   "dataConstituicao": "2024-02-25", "valorIssqn": "cem reais",
                   "tipoCredito": "ISSQN", "simplesNacional": "Sim", "aliquota": 5.0,
                   "valorFaturado": 2010.00, "valorDeducao": 0, "baseCalculo": 2010.00 }]
                """;

            await AssertRejectedWithoutPublishingAsync(
                () => _api.PostRawAsync(bodyWithTextInsteadOfDecimal),
                expectedBodyFragment: "errors");
        }

        /// <summary>CI-02 (negativo): corpo que não é JSON válido responde 400, nunca 500.</summary>
        [Test]
        [Description("CI-02: corpo que não é JSON válido responde 400 (nunca 500) e nada é publicado")]
        public async Task shouldReturn400NotFiveHundredWhenBodyIsNotValidJson()
        {
            await AssertRejectedWithoutPublishingAsync(
                () => _api.PostRawAsync("{ isto não é json"),
                expectedBodyFragment: "errors");
        }

        /// <summary>CI-02 (negativo): corpo JSON que não é um array responde 400, nunca 500.</summary>
        [Test]
        [Description("CI-02: corpo JSON que não é um array responde 400 (nunca 500) e nada é publicado")]
        public async Task shouldReturn400NotFiveHundredWhenBodyIsNotAnArray()
        {
            var singleObjectInsteadOfArray =
                $$"""
                { "numeroCredito": "{{UniqueIds.NewNumeroCredito()}}", "numeroNfse": "{{UniqueIds.NewNumeroNfse()}}" }
                """;

            await AssertRejectedWithoutPublishingAsync(
                () => _api.PostRawAsync(singleObjectInsteadOfArray),
                expectedBodyFragment: "errors");
        }

        /// <summary>
        /// Executa o POST inválido com o probe posicionado ANTES dele e afirma: 400, fragmento esperado
        /// no corpo, nenhum "success" e nenhuma mensagem nova no tópico real durante a janela.
        /// </summary>
        private static async Task AssertRejectedWithoutPublishingAsync(
            Func<Task<HttpResponseMessage>> post,
            string expectedBodyFragment)
        {
            using var probe = new KafkaTopicProbe();
            probe.CaptureBaseline();

            using var response = await post();
            var body = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest, $"corpo da resposta: {body}");
            body.Should().Contain(expectedBodyFragment);
            body.Should().NotContain("success");

            var published = probe.CollectNewMessages(E2ESettings.KafkaSilenceWindow);

            published.Should().BeEmpty(
                $"um POST inválido nunca pode publicar no tópico '{E2ESettings.InboundTopic}'; " +
                $"mensagens observadas em {E2ESettings.KafkaSilenceWindow.TotalSeconds:F0}s após a baseline " +
                $"{string.Join(", ", probe.Baseline.Select(b => $"{b.Partition.Value}@{b.Offset.Value}"))}: " +
                $"[{string.Join(" | ", published)}]");
        }
    }
}
