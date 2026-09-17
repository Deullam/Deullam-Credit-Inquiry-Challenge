using System.Net;
using Deullam.Credit.Inquiry.Challenge.E2E.Tests.Setup;
using FluentAssertions;

namespace Deullam.Credit.Inquiry.Challenge.E2E.Tests.Features.GerenciarCredito
{
    /// <summary>
    /// CI-04 — Duplicata não é gravada duas vezes e o consumidor sobrevive ao conflito. Tudo via API
    /// pública (caixa-preta): a "não duplicação" é lida na listagem por NFS-e, e a sobrevivência do
    /// consumidor é provada por um crédito posterior (Z) que continua sendo processado.
    /// </summary>
    [TestFixture]
    [Category("Core")]
    [Category("CI-04")]
    public class Ci04DuplicateCreditTests
    {
        private CreditApiClient _api = null!;

        /// <summary>Cria o cliente da API sobre o HttpClient compartilhado da suíte.</summary>
        [OneTimeSetUp]
        public void CreateApiClient() => _api = new CreditApiClient(RealStackFixture.Http);

        /// <summary>
        /// CI-04/AC1..AC3 em um único cenário, porque os passos são dependentes:
        /// <list type="number">
        ///   <item>POST do crédito X (NFS-e Y) e espera o consumidor gravar.</item>
        ///   <item>POST de novo do mesmo X, com ValorIssqn diferente, simulando reenvio: ainda 202 (a
        ///   deduplicação é do consumidor, não da borda). O probe Kafka confirma que a duplicata foi
        ///   de fato publicada — o consumidor precisou enfrentá-la.</item>
        ///   <item>POST de Z na mesma NFS-e Y, depois da duplicata. Z ficar consultável prova que o laço
        ///   de consumo continuou depois da ConflictException. Como o tópico tem uma partição e o
        ///   consumidor processa em ordem, Z visível implica a duplicata já consumida — sem esperar
        ///   um orçamento fixo.</item>
        ///   <item>GET por NFS-e Y: exatamente UMA ocorrência de X, com os valores do ORIGINAL (o
        ///   CreateIfNotExistsAsync só insere, nunca atualiza), e Z presente.</item>
        /// </list>
        /// </summary>
        [Test]
        [Description("CI-04: reenvio do mesmo numeroCredito responde 202, não cria segunda linha, não altera o original e o consumidor segue processando")]
        public async Task shouldNotDuplicateCreditAndKeepConsumingAfterConflict()
        {
            var nfse = UniqueIds.NewNumeroNfse();
            var original = CreditBuilder.AUniqueCredit().WithNumeroNfse(nfse).WithValorIssqn(111.11m).Build();

            // 1. Original X processado.
            using (var accepted = await _api.PostCreditsAsync(new[] { original }))
            {
                accepted.StatusCode.Should().Be(HttpStatusCode.Accepted);
            }
            var stored = await _api.WaitUntilCreditIsQueryableAsync(original.NumeroCredito);
            stored.ValorIssqn.Should().Be(original.ValorIssqn);

            // 2. Duplicata de X com valor diferente: a borda aceita (202) e publica no tópico real.
            var duplicate = CreditBuilder.AUniqueCredit()
                .WithNumeroCredito(original.NumeroCredito)
                .WithNumeroNfse(nfse)
                .WithValorIssqn(999.99m)
                .Build();

            using var probe = new KafkaTopicProbe();
            probe.CaptureBaseline();

            using (var acceptedAgain = await _api.PostCreditsAsync(new[] { duplicate }))
            {
                acceptedAgain.StatusCode.Should().Be(
                    HttpStatusCode.Accepted,
                    "a deduplicação é responsabilidade do consumidor, não da borda (README)");
            }

            var publishedDuplicate = probe.CollectNewMessages(E2ESettings.ConsumerBudget, stopWhen: seen => seen.Count >= 1);
            publishedDuplicate.Should().ContainSingle(payload => payload.Contains(original.NumeroCredito),
                "a duplicata precisa chegar ao tópico real para o consumidor ter de lidar com ela");

            // 3. Z depois da duplicata: o consumidor precisa continuar vivo para processá-lo.
            var later = CreditBuilder.AUniqueCredit().WithNumeroNfse(nfse).WithValorIssqn(222.22m).Build();
            using (var acceptedLater = await _api.PostCreditsAsync(new[] { later }))
            {
                acceptedLater.StatusCode.Should().Be(HttpStatusCode.Accepted);
            }

            var storedLater = await _api.WaitUntilCreditIsQueryableAsync(later.NumeroCredito);
            storedLater.Should().BeEquivalentTo(later);

            // 4. Uma única linha para X, com os valores do original; Z presente na mesma NFS-e.
            using var byNfse = await _api.GetByNfseAsync(nfse);
            byNfse.StatusCode.Should().Be(HttpStatusCode.OK);
            var credits = await CreditApiClient.ReadCreditListAsync(byNfse);

            credits.Should().HaveCount(2, "só X (uma vez) e Z pertencem a esta NFS-e");
            credits.Where(c => c.NumeroCredito == original.NumeroCredito)
                .Should().ContainSingle("o reenvio do mesmo numeroCredito nunca gera uma segunda linha")
                .Which.Should().BeEquivalentTo(original, "CreateIfNotExistsAsync só insere, nunca atualiza");
            credits.Should().ContainSingle(c => c.NumeroCredito == later.NumeroCredito);

            using var byNumero = await _api.GetByNumeroCreditoAsync(original.NumeroCredito);
            var unchanged = await CreditApiClient.ReadCreditAsync(byNumero);
            unchanged.ValorIssqn.Should().Be(original.ValorIssqn, "a duplicata (999.99) não pode sobrescrever o original");
        }
    }
}
