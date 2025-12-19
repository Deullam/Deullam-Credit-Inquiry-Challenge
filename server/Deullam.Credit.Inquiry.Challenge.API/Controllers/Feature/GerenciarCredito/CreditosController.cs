using Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito;
using Deullam.Credit.Inquiry.Challenge.Application.Messaging;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Deullam.Credit.Inquiry.Challenge.API.Controllers.Feature.GerenciarCredito
{
    /// <summary>
    /// Controller responsável por gerenciar as operações relacionadas a Créditos Constituídos.
    /// </summary>
    [ApiController]
    [Route("api/creditos")]
    public class CreditosController : ControllerBase
    {
        private readonly ICreditoService _creditoService;
        private readonly IMessagePublisher _messagePublisher;
        private const string TopicName = "integrar-credito-constituido-entry";

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CreditosController"/>.
        /// </summary>
        /// <param name="creditoService">O serviço para operações de consulta de crédito.</param>
        /// <param name="messagePublisher">O serviço para publicar mensagens para integração.</param>
        public CreditosController(ICreditoService creditoService, IMessagePublisher messagePublisher)
        {
            _creditoService = creditoService;
            _messagePublisher = messagePublisher;
        }

        /// <summary>
        /// Retorna os detalhes de um crédito específico com base no seu número.
        /// Rota: GET /api/creditos/credito/{numeroCredito}
        /// </summary>
        [HttpGet("credito/{numeroCredito}")]
        [ProducesResponseType(typeof(CreditoDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByNumeroCredito(string numeroCredito)
        {
            var creditoDto = await _creditoService.GetByNumeroCreditoAsync(numeroCredito);
            return Ok(creditoDto);
        }

        /// <summary>
        /// Retorna uma lista de créditos constituídos com base no número da NFS-e.
        /// Rota: GET /api/creditos/nfse/{numeroNfse}
        /// </summary>
        [HttpGet("nfse/{numeroNfse}")]
        [ProducesResponseType(typeof(IEnumerable<CreditoDto>), 200)]
        public async Task<IActionResult> GetByNfse(string numeroNfse)
        {
            var creditosDto = await _creditoService.GetByNfseAsync(numeroNfse);
            return Ok(creditosDto);
        }




        /// <summary>
        /// Enfileira uma lista de créditos para integração assíncrona.
        /// </summary>
        /// <remarks>
        /// Este endpoint recebe uma lista de objetos de crédito e os publica individualmente em um tópico do Kafka
        /// para serem processados por um serviço de background. O processamento inclui a validação e a inserção
        /// no banco de dados, caso o crédito ainda não exista.
        ///
        /// A resposta é imediata após o enfileiramento, não aguardando a conclusão do processamento.
        ///
        /// **Exemplo de requisição:**
        ///
        ///     POST /api/creditos/integrar-credito-constituido
        ///     [
        ///       {
        ///         "numeroCredito": "123456",
        ///         "numeroNfse": "7891011",
        ///         "dataConstituicao": "2024-02-25",
        ///         "valorIssqn": 1500.75,
        ///         "tipoCredito": "ISSQN",
        ///         "simplesNacional": "Sim",
        ///         "aliquota": 5.0,
        ///         "valorFaturado": 30000.00,
        ///         "valorDeducao": 5000.00,
        ///         "baseCalculo": 25000.00
        ///       }
        ///     ]
        ///
        /// </remarks>
        /// <param name="creditos">Uma lista de objetos `CreditoDto` contendo os dados dos créditos a serem integrados.</param>
        /// <returns>Um status de sucesso que indica que os créditos foram recebidos e enfileirados.</returns>
        /// <response code="202">Accepted - Retorna quando a lista de créditos é recebida com sucesso e as mensagens são enfileiradas para processamento.</response>
        /// <response code="400">Bad Request - Retorna se a lista de créditos enviada for nula ou vazia.</response>
        /// <response code="500">Internal Server Error - Retorna se ocorrer um erro inesperado durante a publicação no serviço de mensageria.</response>
        [HttpPost("integrar-credito-constituido")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("integrar-credito-constituido")]
        public async Task<IActionResult> IntegrarCreditoConstituido([FromBody] List<CreditoDto> creditos)
        {
            if (creditos == null || !creditos.Any())
            {
                return BadRequest("A lista de créditos não pode ser nula ou vazia.");
            }

            foreach (var credito in creditos)
            {
                var message = JsonSerializer.Serialize(credito);
                await _messagePublisher.PublishAsync(TopicName, message);
            }

            return Accepted(new { success = true });
        }




    }
}

