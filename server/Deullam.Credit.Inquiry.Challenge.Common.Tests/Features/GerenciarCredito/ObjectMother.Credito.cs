// Namespace: Deullam.Credit.Inquiry.Challenge.Common.Tests.Features.GerenciarCredito
namespace Deullam.Credit.Inquiry.Challenge.Common.Tests.Features.GerenciarCredito
{
    using Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito;
    using Deullam.Credit.Inquiry.Challenge.Domain.Features.GerenciarCredito;
    using System;

    public static partial class ObjectMother
    {
        /// <summary>
        /// Cria uma instância padrão de um Crédito para uso em testes.
        /// </summary>
        public static Credito GetDefaultCredito()
        {
            return new Credito
            {
                Id = 1,
                NumeroCredito = "123456",
                NumeroNfse = "7891011",
                DataConstituicao = new DateTime(2024, 2, 25),
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
        /// Cria uma lista de créditos para testes de múltiplos resultados.
        /// </summary>
        public static List<Credito> GetDefaultCreditoList()
        {
            return new List<Credito>
            {
            // Reutiliza o método que já temos para o primeiro item
                GetDefaultCredito(), 
        
                // Adiciona um segundo crédito para simular uma lista
                new Credito
                {
                    Id = 2,
                    NumeroCredito = "654321",
                    NumeroNfse = "7891011",
                    DataConstituicao = new DateTime(2024, 3, 10),
                    ValorIssqn = 800.50m,
                    TipoCredito = "Outros",
                    SimplesNacional = false,
                    Aliquota = 3.5m,
                    ValorFaturado = 20000.00m,
                    ValorDeducao = 3000.00m,
                    BaseCalculo = 17000.00m
                }
            };
        }


        // Adicione este método à partial class ObjectMother

        /// <summary>
        /// Cria uma instância padrão de um CreditoDto para uso em testes.
        /// </summary>
        public static CreditoDto GetDefaultCreditoDto()
        {
            return new CreditoDto
            {
                NumeroCredito = "123456",
                NumeroNfse = "7891011",
                DataConstituicao = new DateTime(2024, 2, 25),
                ValorIssqn = 1500.75m,
                TipoCredito = "ISSQN",
                SimplesNacional = "Sim", // O DTO usa string
                Aliquota = 5.0m,
                ValorFaturado = 30000.00m,
                ValorDeducao = 5000.00m,
                BaseCalculo = 25000.00m
            };
        }

    }
}
