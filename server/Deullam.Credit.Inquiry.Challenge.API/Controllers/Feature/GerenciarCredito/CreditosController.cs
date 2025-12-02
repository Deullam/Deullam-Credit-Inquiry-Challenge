using Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Namespace corrigido conforme sua especificação.
namespace Deullam.Credit.Inquiry.Challenge.API.Controllers.Feature.GerenciarCredito
{
    /// <summary>
    /// Controller responsável por gerenciar as operações relacionadas a Créditos Constituídos.
    /// </summary>
    [ApiController]
    [Route("api/creditos")] // Rota base corrigida para corresponder ao requisito.
    public class CreditosController : ControllerBase
    {
        private readonly ICreditoService _creditoService;

        public CreditosController(ICreditoService creditoService)
        {
            _creditoService = creditoService;
        }

        /// <summary>
        /// Retorna os detalhes de um crédito específico com base no seu número.
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
        /// </summary>
        [HttpGet("{numeroNfse}")]
        [ProducesResponseType(typeof(IEnumerable<CreditoDto>), 200)]
        public async Task<IActionResult> GetByNfse(string numeroNfse)
        {
            var creditosDto = await _creditoService.GetByNfseAsync(numeroNfse);
            return Ok(creditosDto);
        }

        /// <summary>
        /// Integra uma lista de créditos constituídos.
        /// </summary>
        [HttpPost("integrar-credito-constituido")]
        [ProducesResponseType(typeof(object), 202)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> IntegrarCreditoConstituido([FromBody] List<CreditoDto> creditos)
        {
            if (creditos == null || !creditos.Any())
            {
                return BadRequest("A lista de créditos não pode ser nula ou vazia.");
            }

            // Agora esta chamada funciona, pois o método existe no serviço.
            await _creditoService.IntegrarCreditosAsync(creditos);

            return Accepted(new { success = true });
        }
    }
}
