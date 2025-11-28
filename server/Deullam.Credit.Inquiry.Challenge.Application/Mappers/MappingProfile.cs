// Namespace: Deullam.Credit.Inquiry.Challenge.Application.AutoMapper
using AutoMapper;
using Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito;
using Deullam.Credit.Inquiry.Challenge.Domain.Features.GerenciarCredito;

namespace Deullam.Credit.Inquiry.Challenge.Application.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapeamento de Credito (Entidade) para CreditoDto
            CreateMap<Credito, CreditoDto>()
                // Regra customizada para converter bool para string "Sim"/"Não"
                .ForMember(dest => dest.SimplesNacional, opt => opt.MapFrom(src => src.SimplesNacional ? "Sim" : "Não"));

            // Mapeamento de CreditoDto para Credito (Entidade)
            CreateMap<CreditoDto, Credito>()
                // Regra customizada para converter string "Sim"/"Não" para bool
                .ForMember(dest => dest.SimplesNacional, opt => opt.MapFrom(src => src.SimplesNacional.Equals("Sim", StringComparison.OrdinalIgnoreCase)));
        }
    }
}
