using System.Diagnostics;
using System.Net;
using Deullam.Credit.Inquiry.Challenge.E2E.Tests.Setup;

// Namespace raiz de propósito: um [SetUpFixture] vale para todos os testes do namespace em que é
// declarado e dos namespaces abaixo dele.
namespace Deullam.Credit.Inquiry.Challenge.E2E.Tests
{
    /// <summary>
    /// Porteiro da suíte: antes de qualquer teste, espera a API real responder 200 em
    /// <c>/health/ready</c> dentro de <see cref="E2ESettings.ReadinessBudget"/>. Se não responder,
    /// FALHA alto com a instrução de subir a pilha (<c>make up</c> + <c>make migrate</c>) — nunca
    /// pula em silêncio, porque uma suíte verde sem pilha seria mentira.
    /// </summary>
    [SetUpFixture]
    public class RealStackFixture
    {
        /// <summary>Cliente HTTP compartilhado por toda a suíte, já apontando para a API real.</summary>
        public static HttpClient Http { get; private set; } = null!;

        /// <summary>Espera a pilha real ficar pronta ou falha explicando como subi-la.</summary>
        [OneTimeSetUp]
        public async Task WaitForRealStackAsync()
        {
            Http = CreditApiClient.CreateHttpClient();

            var stopwatch = Stopwatch.StartNew();
            var lastObservation = "nenhuma tentativa concluída";

            while (stopwatch.Elapsed < E2ESettings.ReadinessBudget)
            {
                try
                {
                    using var response = await Http.GetAsync("/health/ready");
                    var body = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        TestContext.Progress.WriteLine(
                            $"[E2E] pilha real pronta em {stopwatch.Elapsed.TotalSeconds:F1}s " +
                            $"(API {E2ESettings.ApiBaseUrl}, Kafka {E2ESettings.KafkaBootstrapServers}, run {E2ESettings.RunId}).");
                        return;
                    }

                    lastObservation = $"HTTP {(int)response.StatusCode} em /health/ready: {body}";
                }
                catch (HttpRequestException ex)
                {
                    // Conexão recusada enquanto o contêiner ainda sobe: continua esperando dentro do orçamento.
                    lastObservation = $"sem conexão: {ex.Message}";
                }
                catch (TaskCanceledException ex)
                {
                    lastObservation = $"timeout da chamada HTTP: {ex.Message}";
                }

                await Task.Delay(E2ESettings.ReadinessInterval);
            }

            Assert.Fail(
                $"A API real não respondeu 200 em {E2ESettings.ApiBaseUrl}health/ready dentro de " +
                $"{E2ESettings.ReadinessBudget.TotalSeconds:F0}s. Última observação: {lastObservation}. " +
                "Suba a pilha completa com `cd server && make up && make migrate` antes de rodar esta suíte " +
                $"(ou aponte {E2ESettings.ApiBaseUrlVariable} / {E2ESettings.KafkaBootstrapVariable} para outra pilha).");
        }

        /// <summary>Libera o cliente HTTP compartilhado.</summary>
        [OneTimeTearDown]
        public void DisposeHttpClient() => Http?.Dispose();
    }
}
