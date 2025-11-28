using Deullam.Credit.Inquiry.Challenge.Domain.Features.GerenciarCredito;

namespace Deullam.Credit.Inquiry.Challenge.Domain.Features.GerenciarCredito
{
    public interface ICreditoRepository
    {
        Task<IEnumerable<Credito>> GetByNfseAsync(string numeroNfse);
        Task<Credito> GetByNumeroCreditoAsync(string numeroCredito);
        Task<bool> ExistsByNumeroCreditoAsync(string numeroCredito);
        Task AddAsync(Credito credito);


    }
}
