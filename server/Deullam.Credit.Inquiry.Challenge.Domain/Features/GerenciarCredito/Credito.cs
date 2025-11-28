namespace Deullam.Credit.Inquiry.Challenge.Domain.Features.GerenciarCredito
{
    /// <summary>
    /// Classe do Crédito Constituído.
    /// </summary>
    public class Credito
    {
        public long Id { get; set; }

        public string NumeroCredito { get; set; }

        public string NumeroNfse { get; set; }

        public DateTime DataConstituicao { get; set; }

        public decimal ValorIssqn { get; set; }

        public string TipoCredito { get; set; }

        public bool SimplesNacional { get; set; }

        public decimal Aliquota { get; set; }

        public decimal ValorFaturado { get; set; }

        public decimal ValorDeducao { get; set; }

        public decimal BaseCalculo { get; set; }
    }
}