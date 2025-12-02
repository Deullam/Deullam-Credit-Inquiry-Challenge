using Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito;
using Microsoft.AspNetCore.Mvc;

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

        public CreditosController(ICreditoService creditoService)
        {
            _creditoService = creditoService;
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
        /// Integra uma lista de créditos constituídos.
        /// Rota: POST /api/creditos/integrar-credito-constituido
        /// </summary>
        [HttpPost("integrar-credito-constituido")]
        [ProducesResponseType(typeof(object), 202)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> IntegrarCreditoConstituido([FromBody] List<CreditoDto> creditos)
        {
            if (creditos == null || !creditos.Any())
            {
                return BadRequest("A lista de créditos não pode ser nula ou vazia.");
            }

            await _creditoService.IntegrarCreditosAsync(creditos);

            return Accepted(new { success = true });
        }
    }
}

