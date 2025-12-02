using Deullam.Credit.Inquiry.Challenge.Domain.Features.GerenciarCredito;
using Deullam.Credit.Inquiry.Challenge.Infra.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Deullam.Credit.Inquiry.Challenge.Infra.Data.Features.GerenciarCredito
{
    public class CreditoRepository : ICreditoRepository
    {
        private readonly AppDbContext _dbcontext;

        public CreditoRepository(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        /// <summary>
        /// Adiciona uma nova entidade de Crédito ao banco de dados.
        /// </summary>
        public async Task AddAsync(Credito credito)
        {
            await _dbcontext.Creditos.AddAsync(credito);
            await _dbcontext.SaveChangesAsync();
        }

        /// <summary>
        /// Verifica de forma otimizada se um crédito com o número especificado já existe.
        /// Usa AnyAsync, que é traduzido para uma consulta "EXISTS" no SQL,
        /// sendo muito mais eficiente do que buscar o objeto inteiro.
        /// </summary>
        public async Task<bool> ExistsByNumeroCreditoAsync(string numeroCredito)
        {
            return await _dbcontext.Creditos.AnyAsync(c => c.NumeroCredito == numeroCredito);
        }

        /// <summary>
        /// Busca um crédito único pelo seu número.
        /// Usa FirstOrDefaultAsync para buscar o primeiro registro que corresponde à condição
        /// ou retornar nulo se nenhum for encontrado.
        /// </summary>
        public async Task<Credito> GetByNumeroCreditoAsync(string numeroCredito)
        {
            // 
            return await _dbcontext.Creditos.FirstOrDefaultAsync(c => c.NumeroCredito == numeroCredito);
        }

        /// <summary>
        /// Busca uma lista de créditos associados a uma NFS-e.
        ///   Usa Where para filtrar os registros e ToListAsync para materializar a consulta
        /// e retornar a lista de resultados.
        /// </summary>
        public async Task<IEnumerable<Credito>> GetByNfseAsync(string numeroNfse)
        {

            return await _dbcontext.Creditos.Where(c => c.NumeroNfse == numeroNfse).ToListAsync();
        }
    }
}
