using Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito;
using Deullam.Credit.Inquiry.Challenge.Common.Tests.Features.GerenciarCredito;

namespace Deullam.Credit.Inquiry.Challenge.E2E.Tests.Setup
{
    /// <summary>
    /// Builder de <see cref="CreditoDto"/> para a suíte E2E. Parte do <see cref="ObjectMother"/>
    /// compartilhado e troca os identificadores por valores únicos desta execução (ver
    /// <see cref="UniqueIds"/>), para nunca colidir com o seed das migrations (123456, 789012,
    /// 654321 / NFS-e 7891011, 1122334) nem com execuções anteriores contra o mesmo banco.
    /// </summary>
    public sealed class CreditBuilder
    {
        /// <summary>
        /// Data fixa e sem fuso (Kind Unspecified) para a comparação no GET ser exata: a coluna é
        /// DATE e o JSON vai e volta como "2024-03-10T00:00:00".
        /// </summary>
        public static readonly DateTime DefaultDataConstituicao = new(2024, 3, 10);

        private readonly CreditoDto _credit;

        private CreditBuilder()
        {
            _credit = ObjectMother.GetDefaultCreditoDto();
            _credit.NumeroCredito = UniqueIds.NewNumeroCredito();
            _credit.NumeroNfse = UniqueIds.NewNumeroNfse();
            _credit.DataConstituicao = DefaultDataConstituicao;
        }

        /// <summary>Um crédito válido com numeroCredito e numeroNfse únicos desta execução.</summary>
        public static CreditBuilder AUniqueCredit() => new();

        /// <summary>Usa um numeroCredito específico (ex.: para simular reenvio/duplicata).</summary>
        public CreditBuilder WithNumeroCredito(string numeroCredito)
        {
            _credit.NumeroCredito = numeroCredito;
            return this;
        }

        /// <summary>Usa um numeroNfse específico (ex.: vários créditos na mesma NFS-e).</summary>
        public CreditBuilder WithNumeroNfse(string numeroNfse)
        {
            _credit.NumeroNfse = numeroNfse;
            return this;
        }

        /// <summary>Define o campo SimplesNacional ("Sim" ou "Não" no contrato da API).</summary>
        public CreditBuilder WithSimplesNacional(string simplesNacional)
        {
            _credit.SimplesNacional = simplesNacional;
            return this;
        }

        /// <summary>Define o valor do ISSQN (útil para provar que uma duplicata não sobrescreve o original).</summary>
        public CreditBuilder WithValorIssqn(decimal valorIssqn)
        {
            _credit.ValorIssqn = valorIssqn;
            return this;
        }

        /// <summary>Devolve o DTO montado.</summary>
        public CreditoDto Build() => _credit;
    }

    /// <summary>
    /// Gera identificadores únicos por execução: prefixo <c>E2E-{RunId}</c> + sufixo aleatório.
    /// Cabem na coluna de 50 caracteres e nunca coincidem com o seed nem com outra execução.
    /// </summary>
    public static class UniqueIds
    {
        /// <summary>Novo numeroCredito único (ex.: <c>E2E-1a2b3c4d-C-9f8e7d6c</c>).</summary>
        public static string NewNumeroCredito() => $"E2E-{E2ESettings.RunId}-C-{Suffix()}";

        /// <summary>Novo numeroNfse único (ex.: <c>E2E-1a2b3c4d-N-9f8e7d6c</c>).</summary>
        public static string NewNumeroNfse() => $"E2E-{E2ESettings.RunId}-N-{Suffix()}";

        private static string Suffix() => Guid.NewGuid().ToString("N")[..8];
    }
}
