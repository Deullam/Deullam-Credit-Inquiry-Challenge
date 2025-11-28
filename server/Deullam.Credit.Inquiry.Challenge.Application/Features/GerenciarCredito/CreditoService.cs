using AutoMapper;
using Deullam.Credit.Inquiry.Challenge.Domain.Features.GerenciarCredito;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito
{
    public class CreditoService : ICreditoService // <-- Implementa a interface
    {
        private readonly ICreditoRepository _creditoRepository;
        private readonly IMapper _mapper;

        public CreditoService(ICreditoRepository creditoRepository, IMapper mapper)
        {
            _creditoRepository = creditoRepository;
            _mapper = mapper;
        }

        // Assinatura corresponde à interface: Task<IEnumerable<CreditoDto>> GetByNfseAsync(string numeroNfse)
        public async Task<IEnumerable<CreditoDto>> GetByNfseAsync(string numeroNfse)
        {
            var creditosEntidades = await _creditoRepository.GetByNfseAsync(numeroNfse);
            return _mapper.Map<IEnumerable<CreditoDto>>(creditosEntidades);
        }

        // Assinatura corresponde à interface: Task<CreditoDto> GetByNumeroCreditoAsync(string numeroCredito)
        public async Task<CreditoDto> GetByNumeroCreditoAsync(string numeroCredito)
        {
            var creditoEntidade = await _creditoRepository.GetByNumeroCreditoAsync(numeroCredito);
            return _mapper.Map<CreditoDto>(creditoEntidade);
        }

        // Assinatura corresponde à interface: Task CreateIfNotExistsAsync(CreditoDto creditoDto)
        public async Task CreateIfNotExistsAsync(CreditoDto creditoDto)
        {
            var exists = await _creditoRepository.ExistsByNumeroCreditoAsync(creditoDto.NumeroCredito);
            if (!exists)
            {
                var creditoEntidade = _mapper.Map<Credito>(creditoDto);
                await _creditoRepository.AddAsync(creditoEntidade);
            }
        }
    }
}
