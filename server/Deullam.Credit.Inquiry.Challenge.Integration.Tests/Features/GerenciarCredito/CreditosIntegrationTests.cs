using Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito;
using Deullam.Credit.Inquiry.Challenge.Common.Tests.Features.GerenciarCredito;
using FluentAssertions;
using global::Deullam.Credit.Inquiry.Challenge.Integration.Tests.Setup;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace Deullam.Credit.Inquiry.Challenge.Integration.Tests.Features.GerenciarCredito
{
    [TestFixture]
    public class CreditosControllerTests
    {
        private WebAppFactory _factory;
        private HttpClient _client;
        private PostgreSqlContainer _dbContainer;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _dbContainer = new PostgreSqlBuilder()
                .WithImage("postgres:15-alpine")
                .WithDatabase("test_db")
                .WithUsername("test_user")
                .WithPassword("test_pass")
                .Build();
            await _dbContainer.StartAsync();

            _factory = new WebAppFactory(_dbContainer.GetConnectionString());
            _client = _factory.CreateClient();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            _client.Dispose();
            _factory.Dispose();
            await _dbContainer.StopAsync();
            await _dbContainer.DisposeAsync();
        }

        /// <summary>
        /// Testa se o endpoint de integração retorna 202 Accepted ao receber dados válidos.
        /// </summary>
        [Test, Order(1)]
        public async Task Post_IntegrarCredito_ComDadosValidos_DeveRetornarAccepted()
        {
            var creditosParaIntegrar = ObjectMother.GetDefaultCreditoDtoList();

            var response = await _client.PostAsJsonAsync("/api/creditos/integrar-credito-constituido", creditosParaIntegrar);

            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// Testa se é possível buscar um crédito recém-inserido e se os dados estão corretos.
        /// </summary>
        [Test, Order(2)]
        public async Task Get_GetByNumeroCredito_AposInsercao_DeveRetornarOkComDadosCorretos()
        {
            var creditoOriginal = ObjectMother.GetDefaultCreditoDto();

            var response = await _client.GetAsync($"/api/creditos/credito/{creditoOriginal.NumeroCredito}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var creditoRetornado = await response.Content.ReadFromJsonAsync<CreditoDto>();
            creditoRetornado.Should().NotBeNull();
            creditoRetornado.NumeroCredito.Should().Be(creditoOriginal.NumeroCredito);
        }

        /// <summary>
        /// Testa se o sistema previne a inserção de um crédito duplicado, retornando 409 Conflict.
        /// </summary>
        [Test, Order(3)]
        public async Task Post_IntegrarCredito_ComCreditoDuplicado_DeveRetornarConflict()
        {
            var creditoDuplicado = ObjectMother.GetDefaultCreditoDtoList();

            var response = await _client.PostAsJsonAsync("/api/creditos/integrar-credito-constituido", creditoDuplicado);

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        /// <summary>
        /// Testa se a busca por um número de crédito que não existe retorna 404 Not Found.
        /// </summary>
        [Test, Order(4)]
        public async Task Get_GetByNumeroCredito_ComNumeroInexistente_DeveRetornarNotFound()
        {
            var numeroInexistente = "NAO-EXISTE-123";

            var response = await _client.GetAsync($"/api/creditos/credito/{numeroInexistente}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Testa se o sistema rejeita dados com valor de ISSQN inválido, retornando 422 Unprocessable Entity.
        /// </summary>
        [Test, Order(5)]
        public async Task Post_IntegrarCredito_ComValorIssqnInvalido_DeveRetornarUnprocessableEntity()
        {
            var creditoInvalidoDto = ObjectMother.GetDtoComValorIssqnInvalido();
            creditoInvalidoDto.NumeroCredito = "INT-TEST-INVALIDO-002";
            var listaParaIntegrar = new List<CreditoDto> { creditoInvalidoDto };

            var response = await _client.PostAsJsonAsync("/api/creditos/integrar-credito-constituido", listaParaIntegrar);

            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var errorResponse = await response.Content.ReadAsStringAsync();
            errorResponse.Should().Contain("O valor do ISSQN deve ser maior que zero.");
        }
    }
}

