namespace Deullam.Credit.Inquiry.Challenge.Domain.Tests.Features.GerenciarCredito
{
    using Deullam.Credit.Inquiry.Challenge.Common.Tests.Features.GerenciarCredito;
    using Deullam.Credit.Inquiry.Challenge.Domain.Features.GerenciarCredito;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class CreditoTests
    {
        [Test(Description = "Deve Criar Crédito Com Sucesso")]
        [Category("Testes de Unidade - Domínio - Credito")]
        public void Credito_DeveSerCriadoComSucesso_QuandoDadosValidosForemFornecidos()
        {
            //Arrange
            Credito creditoTemplate = ObjectMother.GetDefaultCredito();

            // Act
            Credito credito = creditoTemplate;

            // Assert
            credito.Should().NotBeNull();
            credito.NumeroCredito.Should().Be(creditoTemplate.NumeroCredito);
            credito.NumeroNfse.Should().Be(creditoTemplate.NumeroNfse);
            credito.DataConstituicao.Should().Be(creditoTemplate.DataConstituicao);
            credito.ValorIssqn.Should().Be(creditoTemplate.ValorIssqn);
            credito.SimplesNacional.Should().Be(creditoTemplate.SimplesNacional);
        }
    }
}
