namespace Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito
{
    public class CreditoDto
    {
        public required string NumeroCredito { get; set; }
        public required string NumeroNfse { get; set; }
        public DateTime DataConstituicao { get; set; }
        public decimal ValorIssqn { get; set; }
        public required string TipoCredito { get; set; }
        public required string SimplesNacional { get; set; }
        public decimal Aliquota { get; set; }
        public decimal ValorFaturado { get; set; }
        public decimal ValorDeducao { get; set; }
        public decimal BaseCalculo { get; set; }
    }
}
