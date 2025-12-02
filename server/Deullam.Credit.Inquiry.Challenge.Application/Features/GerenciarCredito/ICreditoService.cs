using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito
{
    /// <summary>
    /// Define o contrato para os serviços de negócio relacionados a Créditos.
    /// As operações recebem e retornam DTOs para desacoplar o domínio da apresentação.
    /// </summary>
    public interface ICreditoService
    {
        /// <summary>
        /// Obtém uma lista de créditos (como DTOs) com base no número da NFS-e.
        /// </summary>
        /// <param name="numeroNfse">O número da NFS-e a ser pesquisado.</param>
        /// <returns>Uma coleção de <see cref="CreditoDto"/>.</returns>
        Task<IEnumerable<CreditoDto>> GetByNfseAsync(string numeroNfse);

        /// <summary>
        /// Obtém os detalhes de um crédito específico (como DTO) com base no seu número.
        /// </summary>
        /// <param name="numeroCredito">O número do crédito a ser pesquisado.</param>
        /// <returns>Um <see cref="CreditoDto"/> ou nulo se não for encontrado.</returns>
        Task<CreditoDto> GetByNumeroCreditoAsync(string numeroCredito);

        /// <summary>
        /// Cria um novo crédito na base de dados se ele ainda não existir.
        /// </summary>
        /// <param name="creditoDto">O DTO contendo os dados do crédito a ser criado.</param>
        Task CreateIfNotExistsAsync(CreditoDto creditoDto);

        Task IntegrarCreditosAsync(IEnumerable<CreditoDto> creditosDto);
    }
}
