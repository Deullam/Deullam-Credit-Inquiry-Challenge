// Namespace: Deullam.Credit.Inquiry.Challenge.Common.Tests.Features.GerenciarCredito
namespace Deullam.Credit.Inquiry.Challenge.Common.Tests.Features.GerenciarCredito
{
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
    }
}
