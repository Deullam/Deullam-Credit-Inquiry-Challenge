using Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito;
using Deullam.Credit.Inquiry.Challenge.Domain.Features.GerenciarCredito;

namespace Deullam.Credit.Inquiry.Challenge.Common.Tests.Features.GerenciarCredito
{
    public static partial class ObjectMother
    {
        /// <summary>
        /// Retorna uma instância padrão da entidade Credito com dados válidos.
        /// </summary>
        public static Credito GetDefaultCredito()
        {
            return new Credito
            {
                Id = 1,
                NumeroCredito = "123456",
                NumeroNfse = "7891011",
                DataConstituicao = System.DateTime.Now.Date,
                ValorIssqn = 1500.75m,
                TipoCredito = "ISSQN",
                SimplesNacional = true,
                Aliquota = 5.0m,
                ValorFaturado = 30000.00m,
                ValorDeducao = 5000.00m,
                BaseCalculo = 25000.00m
            };
        }

        /// <summary>
        /// Retorna uma lista padrão contendo uma entidade Credito válida.
        /// </summary>
        public static List<Credito> GetDefaultCreditoList()
        {
            return new List<Credito> { GetDefaultCredito() };
        }

        /// <summary>
        /// Retorna uma instância padrão de CreditoDto com dados válidos.
        /// </summary>
        public static CreditoDto GetDefaultCreditoDto()
        {
            return new CreditoDto
            {
                NumeroCredito = "INT-TEST-001",
                NumeroNfse = "NFSE-999",
                DataConstituicao = System.DateTime.Now.Date,
                ValorIssqn = 100.50m,
                TipoCredito = "ISSQN",
                SimplesNacional = "Sim",
                Aliquota = 5.0m,
                ValorFaturado = 2010.00m,
                ValorDeducao = 0m,
                BaseCalculo = 2010.00m
            };
        }

        /// <summary>
        /// Retorna uma lista padrão contendo um CreditoDto válido.
        /// </summary>
        public static List<CreditoDto> GetDefaultCreditoDtoList()
        {
            return new List<CreditoDto> { GetDefaultCreditoDto() };
        }

        /// <summary>
        /// Retorna um CreditoDto com o Número do Crédito vazio para testes de validação.
        /// </summary>
        public static CreditoDto GetDtoComNumeroCreditoInvalido()
        {
            var dto = GetDefaultCreditoDto();
            dto.NumeroCredito = string.Empty;
            return dto;
        }

        /// <summary>
        /// Retorna um CreditoDto com a Data de Constituição no futuro para testes de validação.
        /// </summary>
        public static CreditoDto GetDtoComDataConstituicaoInvalida()
        {
            var dto = GetDefaultCreditoDto();
            dto.DataConstituicao = System.DateTime.Now.AddDays(1);
            return dto;
        }

        /// <summary>
        /// Retorna um CreditoDto com o Valor do ISSQN negativo para testes de validação.
        /// </summary>
        public static CreditoDto GetDtoComValorIssqnInvalido()
        {
            var dto = GetDefaultCreditoDto();
            dto.ValorIssqn = -100m;
            return dto;
        }

        /// <summary>
        /// Retorna um CreditoDto com o campo Simples Nacional inválido para testes de validação.
        /// </summary>
        public static CreditoDto GetDtoComSimplesNacionalInvalido()
        {
            var dto = GetDefaultCreditoDto();
            dto.SimplesNacional = "Talvez";
            return dto;
        }
    }
}
