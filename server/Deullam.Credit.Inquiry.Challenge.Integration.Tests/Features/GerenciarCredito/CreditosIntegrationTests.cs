using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito;
using Deullam.Credit.Inquiry.Challenge.Common.Tests.Features.GerenciarCredito;
using Deullam.Credit.Inquiry.Challenge.Infra.Data.Contexts;
using Deullam.Credit.Inquiry.Challenge.Integration.Tests.Setup;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Deullam.Credit.Inquiry.Challenge.Integration.Tests.Features.GerenciarCredito
{
    /// <summary>
    /// Testes de integração do fluxo de créditos, ponta a ponta sobre a API real.
    ///
    /// O contrato exercitado é o assíncrono de verdade: o POST publica em uma fila e devolve
    /// 202 Accepted sem esperar a persistência; quem grava no banco é o consumidor. Os testes são
    /// independentes entre si: cada um usa seu próprio número de crédito e nenhum depende da ordem
    /// de execução.
    /// </summary>
    [TestFixture]
    public class CreditosIntegrationTests
    {
        private const string IntegrarUrl = "/api/creditos/integrar-credito-constituido";
        private const string TopicoDeEntrada = "integrar-credito-constituido-entry";

        private ITestDatabase _database = null!;
        private CreditoApiFactory _factory = null!;
        private HttpClient _client = null!;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _database = await TestDatabaseFactory.CreateAsync();
            _factory = new CreditoApiFactory(_database);
            _client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            _database.CreateSchema(scope.ServiceProvider.GetRequiredService<AppDbContext>());

            TestContext.Progress.WriteLine($"Banco da suite de integracao: {_database.Description}");
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            _client.Dispose();
            await _factory.DisposeAsync();
            await _database.DisposeAsync();
        }

        [SetUp]
        public void SetUp() => _factory.PublishedMessages.Clear();

        /// <summary>
        /// O POST aceita a carga e publica exatamente uma mensagem por crédito, no tópico certo e
        /// com o payload íntegro. É o que prova que a API delega a gravação para a mensageria.
        /// </summary>
        [Test]
        public async Task Post_ComListaValida_DeveRetornar202EPublicarUmaMensagemPorCredito()
        {
            var credito = NovoCredito();

            var response = await _client.PostAsJsonAsync(IntegrarUrl, new List<CreditoDto> { credito });

            response.StatusCode.Should().Be(HttpStatusCode.Accepted);

            _factory.PublishedMessages.Messages.Should().HaveCount(1);
            var publicada = _factory.PublishedMessages.Messages.Single();
            publicada.Topic.Should().Be(TopicoDeEntrada);

            var payload = JsonSerializer.Deserialize<CreditoDto>(publicada.Value);
            payload.Should().BeEquivalentTo(credito);
        }

        /// <summary>Uma lista vazia é recusada na borda, antes de gerar mensagem.</summary>
        [Test]
        public async Task Post_ComListaVazia_DeveRetornar400ENaoPublicarNada()
        {
            var response = await _client.PostAsJsonAsync(IntegrarUrl, new List<CreditoDto>());

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await response.Content.ReadAsStringAsync()).Should().Contain("não pode ser nula ou vazia");
            _factory.PublishedMessages.Messages.Should().BeEmpty();
        }

        /// <summary>
        /// Um corpo sem os campos obrigatórios do <see cref="CreditoDto"/> é recusado com 400 e o
        /// detalhe dos erros, também sem gerar mensagem.
        /// </summary>
        [Test]
        public async Task Post_ComCampoObrigatorioAusente_DeveRetornar400ComOsErros()
        {
            const string corpoSemNumeroCredito =
                """
                [{ "numeroNfse": "NFSE-999", "dataConstituicao": "2024-02-25", "valorIssqn": 100.50 }]
                """;

            var response = await _client.PostAsync(
                IntegrarUrl,
                new StringContent(corpoSemNumeroCredito, Encoding.UTF8, "application/json"));

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await response.Content.ReadAsStringAsync()).Should().Contain("errors");
            _factory.PublishedMessages.Messages.Should().BeEmpty();
        }

        /// <summary>Consulta de um crédito que não existe: 404, vindo do middleware de exceção.</summary>
        [Test]
        public async Task Get_ComNumeroCreditoInexistente_DeveRetornar404()
        {
            var response = await _client.GetAsync($"/api/creditos/credito/{Guid.NewGuid():N}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// O ciclo completo: o POST devolve 202 e o recurso ainda NÃO existe; depois que a mensagem
        /// capturada é processada, o GET passa a devolvê-lo.
        /// </summary>
        [Test]
        public async Task Get_AposOConsumidorProcessarAMensagem_DeveRetornarOCredito()
        {
            var credito = NovoCredito();

            var aceite = await _client.PostAsJsonAsync(IntegrarUrl, new List<CreditoDto> { credito });
            aceite.StatusCode.Should().Be(HttpStatusCode.Accepted);

            var antes = await _client.GetAsync($"/api/creditos/credito/{credito.NumeroCredito}");
            antes.StatusCode.Should().Be(
                HttpStatusCode.NotFound,
                "o 202 é um aceite de enfileiramento, não uma confirmação de gravação");

            await ProcessarMensagensCapturadasAsync();

            var depois = await _client.GetAsync($"/api/creditos/credito/{credito.NumeroCredito}");
            depois.StatusCode.Should().Be(HttpStatusCode.OK);

            var retornado = await depois.Content.ReadFromJsonAsync<CreditoDto>();
            retornado.Should().NotBeNull();
            retornado!.NumeroCredito.Should().Be(credito.NumeroCredito);
            retornado.NumeroNfse.Should().Be(credito.NumeroNfse);
            retornado.ValorIssqn.Should().Be(credito.ValorIssqn);
        }

        /// <summary>A consulta por NFS-e devolve os créditos vindos do seed do modelo.</summary>
        [Test]
        public async Task Get_PorNumeroNfse_DeveRetornarOsCreditosDaNfse()
        {
            var response = await _client.GetAsync("/api/creditos/nfse/7891011");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var creditos = await response.Content.ReadFromJsonAsync<List<CreditoDto>>();
            creditos.Should().NotBeNull();
            creditos!.Select(credito => credito.NumeroCredito).Should().Contain(new[] { "123456", "789012" });
        }

        /// <summary>Liveness: a aplicação sobe e responde sem consultar dependência alguma.</summary>
        [Test]
        public async Task HealthSelf_DeveResponderHealthy()
        {
            var response = await _client.GetAsync("/health/self");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var relatorio = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            relatorio.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
        }

        /// <summary>
        /// Readiness: o endpoint responde e reporta as duas dependências críticas.
        ///
        /// O host de teste não tem broker Kafka (e, sem Docker, também não tem PostgreSQL), então o
        /// relatório sai degradado de propósito: 200 quando as dependências estão no ar, 503 quando
        /// não estão. O que se afirma aqui é o que vale nos dois casos: /health/ready está mapeado,
        /// responde no formato do UIResponseWriter e cobre PostgreSQL e Kafka.
        /// </summary>
        [Test]
        public async Task HealthReady_DeveResponderRelatandoPostgreSqlEKafka()
        {
            var response = await _client.GetAsync("/health/ready");

            ((int)response.StatusCode).Should().BeOneOf(
                StatusCodes.Status200OK,
                StatusCodes.Status503ServiceUnavailable);

            using var relatorio = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var entradas = relatorio.RootElement.GetProperty("entries");

            entradas.TryGetProperty("PostgreSQL", out _).Should().BeTrue();
            entradas.TryGetProperty("Kafka", out _).Should().BeTrue();
        }

        /// <summary>
        /// Reproduz o passo de processamento do <c>CreditoConsumerService</c> sem Kafka: desserializa
        /// a mensagem capturada e chama o mesmo <see cref="ICreditoService.CreateIfNotExistsAsync"/>
        /// que o BackgroundService chama, em um escopo de DI próprio.
        /// </summary>
        private async Task ProcessarMensagensCapturadasAsync()
        {
            foreach (var mensagem in _factory.PublishedMessages.Messages)
            {
                var credito = JsonSerializer.Deserialize<CreditoDto>(mensagem.Value);
                credito.Should().NotBeNull();

                using var scope = _factory.Services.CreateScope();
                var creditoService = scope.ServiceProvider.GetRequiredService<ICreditoService>();
                await creditoService.CreateIfNotExistsAsync(credito!);
            }
        }

        /// <summary>Um crédito válido com identificadores exclusivos, para manter os testes independentes.</summary>
        private static CreditoDto NovoCredito()
        {
            var credito = ObjectMother.GetDefaultCreditoDto();
            credito.NumeroCredito = $"INT-{Guid.NewGuid():N}"[..20];
            credito.NumeroNfse = $"NFSE-{Guid.NewGuid():N}"[..20];
            return credito;
        }
    }
}
