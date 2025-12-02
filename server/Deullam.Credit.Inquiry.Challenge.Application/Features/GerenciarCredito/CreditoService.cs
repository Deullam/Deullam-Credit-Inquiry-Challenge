using AutoMapper;
using Deullam.Credit.Inquiry.Challenge.Domain.Exceptions;
using Deullam.Credit.Inquiry.Challenge.Domain.Features.GerenciarCredito;
using FluentValidation;

namespace Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito
{
    public class CreditoService : ICreditoService
    {
        private readonly ICreditoRepository _creditoRepository;
        private readonly IMapper _mapper; 
        private readonly IValidator<CreditoDto> _creditoDtoValidator;

        public CreditoService(
            ICreditoRepository creditoRepository,
            IMapper mapper, 
            IValidator<CreditoDto> creditoDtoValidator)
        {
            _creditoRepository = creditoRepository;
            _mapper = mapper;
            _creditoDtoValidator = creditoDtoValidator;
        }

        public async Task<IEnumerable<CreditoDto>> GetByNfseAsync(string numeroNfse)
        {
            var creditosEntidades = await _creditoRepository.GetByNfseAsync(numeroNfse);
            return _mapper.Map<IEnumerable<CreditoDto>>(creditosEntidades);
        }

        public async Task<CreditoDto> GetByNumeroCreditoAsync(string numeroCredito)
        {
            var creditoEntidade = await _creditoRepository.GetByNumeroCreditoAsync(numeroCredito);
            if (creditoEntidade == null)
            {
                throw new NotFoundException($"Crédito com o número '{numeroCredito}' não foi encontrado.");
            }
            return _mapper.Map<CreditoDto>(creditoEntidade);
        }

        public async Task CreateIfNotExistsAsync(CreditoDto creditoDto)
        {
            var exists = await _creditoRepository.ExistsByNumeroCreditoAsync(creditoDto.NumeroCredito);
            if (exists)
            {
                throw new ConflictException($"O crédito com o número '{creditoDto.NumeroCredito}' já foi integrado anteriormente.");
            }

            var creditoEntidade = _mapper.Map<Credito>(creditoDto);
            await _creditoRepository.AddAsync(creditoEntidade);
        }

        public async Task IntegrarCreditosAsync(IEnumerable<CreditoDto> creditosDto)
        {
            if (creditosDto == null || !creditosDto.Any())
            {
                return; 
            }

            foreach (var creditoDto in creditosDto)
            {
                var validationResult = await _creditoDtoValidator.ValidateAsync(creditoDto);

                if (!validationResult.IsValid)
                {
                    var errorMessages = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
                    throw new UnprocessableEntityException(errorMessages);
                }

                await CreateIfNotExistsAsync(creditoDto);
            }
        }
    }
}
