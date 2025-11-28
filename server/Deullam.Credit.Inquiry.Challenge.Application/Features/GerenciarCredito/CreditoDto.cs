// Namespace: Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito
namespace Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito
{
    public class CreditoDto
    {
        public string NumeroCredito { get; set; }
        public string NumeroNfse { get; set; }
        public DateTime DataConstituicao { get; set; }
        public decimal ValorIssqn { get; set; }
        public string TipoCredito { get; set; }
        public string SimplesNacional { get; set; }
        public decimal Aliquota { get; set; }
        public decimal ValorFaturado { get; set; }
        public decimal ValorDeducao { get; set; }
        public decimal BaseCalculo { get; set; }
    }
}
