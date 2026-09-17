using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito;

namespace Deullam.Credit.Inquiry.Challenge.E2E.Tests.Setup
{
    /// <summary>
    /// Cliente fino sobre <see cref="HttpClient"/> para a API real de créditos. Só conhece rotas e
    /// desserialização; toda asserção fica nos testes. O único comportamento "inteligente" é o
    /// polling de <see cref="WaitUntilCreditIsQueryableAsync"/>, que espera o consumidor real
    /// processar uma mensagem dentro de um orçamento e falha explicitamente ao esgotá-lo.
    /// </summary>
    public sealed class CreditApiClient
    {
        /// <summary>Rota do POST que enfileira créditos para integração assíncrona.</summary>
        public const string IntegrateRoute = "/api/creditos/integrar-credito-constituido";

        /// <summary>Rota do GET por número de crédito (404 quando não existe).</summary>
        public const string CreditRoute = "/api/creditos/credito/";

        /// <summary>Rota do GET por número de NFS-e (200 com lista, vazia quando não há nada).</summary>
        public const string NfseRoute = "/api/creditos/nfse/";

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _http;

        /// <summary>Cria o cliente sobre um <see cref="HttpClient"/> já apontando para a API real.</summary>
        public CreditApiClient(HttpClient http) => _http = http;

        /// <summary>
        /// Cria o <see cref="HttpClient"/> da suíte: base em <see cref="E2ESettings.ApiBaseUrl"/> e
        /// SEM seguir redirecionamentos, para que um 307 do <c>UseHttpsRedirection</c> apareça como
        /// status inesperado em vez de virar erro de TLS obscuro.
        /// </summary>
        public static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler { AllowAutoRedirect = false };
            return new HttpClient(handler)
            {
                BaseAddress = E2ESettings.ApiBaseUrl,
                Timeout = E2ESettings.HttpTimeout
            };
        }

        /// <summary>POST da lista de créditos como JSON (contrato público, camelCase).</summary>
        public Task<HttpResponseMessage> PostCreditsAsync(IEnumerable<CreditoDto> credits)
            => _http.PostAsJsonAsync(IntegrateRoute, credits.ToList(), JsonOptions);

        /// <summary>POST de um corpo bruto, para os casos em que o JSON é inválido de propósito.</summary>
        public Task<HttpResponseMessage> PostRawAsync(string body)
            => _http.PostAsync(IntegrateRoute, new StringContent(body, Encoding.UTF8, "application/json"));

        /// <summary>GET por número de crédito.</summary>
        public Task<HttpResponseMessage> GetByNumeroCreditoAsync(string numeroCredito)
            => _http.GetAsync(CreditRoute + Uri.EscapeDataString(numeroCredito));

        /// <summary>GET por número de NFS-e.</summary>
        public Task<HttpResponseMessage> GetByNfseAsync(string numeroNfse)
            => _http.GetAsync(NfseRoute + Uri.EscapeDataString(numeroNfse));

        /// <summary>GET de um endpoint de health check (<c>/health/self</c> ou <c>/health/ready</c>).</summary>
        public Task<HttpResponseMessage> GetHealthAsync(string path) => _http.GetAsync(path);

        /// <summary>Desserializa um <see cref="CreditoDto"/> do corpo da resposta, falhando se vier vazio.</summary>
        public static async Task<CreditoDto> ReadCreditAsync(HttpResponseMessage response)
        {
            var credit = await response.Content.ReadFromJsonAsync<CreditoDto>(JsonOptions);
            Assert.That(credit, Is.Not.Null, "o corpo da resposta deveria desserializar em CreditoDto");
            return credit!;
        }

        /// <summary>Desserializa a lista de <see cref="CreditoDto"/> do corpo da resposta, falhando se vier nula.</summary>
        public static async Task<List<CreditoDto>> ReadCreditListAsync(HttpResponseMessage response)
        {
            var credits = await response.Content.ReadFromJsonAsync<List<CreditoDto>>(JsonOptions);
            Assert.That(credits, Is.Not.Null, "o corpo da resposta deveria desserializar em List<CreditoDto>");
            return credits!;
        }

        /// <summary>
        /// Faz GET por número de crédito a cada <see cref="E2ESettings.PollingInterval"/> até obter 200
        /// ou esgotar <see cref="E2ESettings.ConsumerBudget"/>. Qualquer status que não seja 404 ou 200
        /// falha na hora (um 500/503 não é "ainda não processou"). Ao esgotar o orçamento, falha
        /// dizendo que o consumidor real não processou a mensagem — nunca vira sucesso por omissão.
        /// </summary>
        public async Task<CreditoDto> WaitUntilCreditIsQueryableAsync(string numeroCredito)
        {
            var stopwatch = Stopwatch.StartNew();
            var attempts = 0;

            while (true)
            {
                attempts++;
                using var response = await GetByNumeroCreditoAsync(numeroCredito);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    TestContext.Progress.WriteLine(
                        $"[E2E] crédito {numeroCredito} consultável após {stopwatch.Elapsed.TotalMilliseconds:F0}ms ({attempts} GET(s)).");
                    return await ReadCreditAsync(response);
                }

                if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    Assert.Fail(
                        $"GET {CreditRoute}{numeroCredito} respondeu {(int)response.StatusCode} durante o polling; " +
                        $"só 404 (ainda não processado) ou 200 (processado) são esperados. Corpo: {body}");
                }

                if (stopwatch.Elapsed >= E2ESettings.ConsumerBudget)
                {
                    Assert.Fail(
                        $"O consumidor real (CreditoConsumerService) não processou o crédito {numeroCredito} em " +
                        $"{E2ESettings.ConsumerBudget.TotalSeconds:F0}s: GET {CreditRoute}{numeroCredito} continuou 404 " +
                        $"após {attempts} tentativas. Verifique `docker compose logs deullam.credit.inquiry.challenge.api` " +
                        $"e se a API está conectada ao mesmo broker do tópico '{E2ESettings.InboundTopic}'.");
                }

                await Task.Delay(E2ESettings.PollingInterval);
            }
        }
    }
}
