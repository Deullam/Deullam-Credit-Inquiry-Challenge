using AutoMapper;
using Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito;
using Deullam.Credit.Inquiry.Challenge.Common.Tests.Features.GerenciarCredito;
using Deullam.Credit.Inquiry.Challenge.Domain.Exceptions;
using Deullam.Credit.Inquiry.Challenge.Domain.Features.GerenciarCredito;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Deullam.Credit.Inquiry.Challenge.Application.Tests.Features.GerenciarCredito
{
    [TestFixture]
    public class CreditoServiceTests
    {
        private Mock<ICreditoRepository> _mockRepository;
        private Mock<IMapper> _mockMapper;
        private Mock<IValidator<CreditoDto>> _mockValidator;
        private ICreditoService _creditoService;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<ICreditoRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockValidator = new Mock<IValidator<CreditoDto>>();

            _creditoService = new CreditoService(
                _mockRepository.Object,
                _mockMapper.Object,
                _mockValidator.Object
            );

           
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<CreditoDto>(), It.IsAny<System.Threading.CancellationToken>()))
                          .ReturnsAsync(new ValidationResult()); 
        }

        [Test]
        public async Task GetByNumeroCreditoAsync_QuandoCreditoExiste_DeveRetornarCreditoDto()
        {
            // Arrange
            var creditoEntidade = ObjectMother.GetDefaultCredito();
            var creditoDto = ObjectMother.GetDefaultCreditoDto();
            _mockRepository.Setup(repo => repo.GetByNumeroCreditoAsync(creditoEntidade.NumeroCredito)).ReturnsAsync(creditoEntidade);
            _mockMapper.Setup(m => m.Map<CreditoDto>(creditoEntidade)).Returns(creditoDto);

            // Act
            var resultado = await _creditoService.GetByNumeroCreditoAsync(creditoEntidade.NumeroCredito);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().BeEquivalentTo(creditoDto);
            _mockRepository.Verify(repo => repo.GetByNumeroCreditoAsync(creditoEntidade.NumeroCredito), Times.Once);
        }

        [Test]
        public void GetByNumeroCreditoAsync_QuandoCreditoNaoExiste_DeveLancarNotFoundException()
        {
            // Arrange
            var numeroCreditoInexistente = "NAO-EXISTE";
            _mockRepository.Setup(repo => repo.GetByNumeroCreditoAsync(numeroCreditoInexistente)).ReturnsAsync((Credito)null);

            // Act
            var acao = async () => await _creditoService.GetByNumeroCreditoAsync(numeroCreditoInexistente);

            // Assert
            acao.Should().ThrowAsync<NotFoundException>();
            _mockRepository.Verify(repo => repo.GetByNumeroCreditoAsync(numeroCreditoInexistente), Times.Once);
        }

        [Test]
        public async Task IntegrarCreditosAsync_QuandoCreditoNaoExiste_DeveChamarAddAsync()
        {
            // Arrange
            var creditoDto = ObjectMother.GetDefaultCreditoDto();
            var creditoEntidade = ObjectMother.GetDefaultCredito();
            _mockRepository.Setup(repo => repo.ExistsByNumeroCreditoAsync(creditoDto.NumeroCredito)).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<Credito>(creditoDto)).Returns(creditoEntidade);

            // Act
            await _creditoService.IntegrarCreditosAsync(new List<CreditoDto> { creditoDto });

            // Assert
            _mockRepository.Verify(repo => repo.AddAsync(creditoEntidade), Times.Once);
        }

        [Test]
        public async Task IntegrarCreditosAsync_QuandoCreditoJaExiste_DeveLancarConflictException()
        {
            // Arrange
            var creditoDto = ObjectMother.GetDefaultCreditoDto();
            _mockRepository.Setup(repo => repo.ExistsByNumeroCreditoAsync(creditoDto.NumeroCredito)).ReturnsAsync(true);

            // Act
            var acao = async () => await _creditoService.IntegrarCreditosAsync(new List<CreditoDto> { creditoDto });

            // Assert
            await acao.Should().ThrowAsync<ConflictException>();
            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Credito>()), Times.Never);
        }

        [Test]
        public void IntegrarCreditosAsync_QuandoDtoInvalido_DeveLancarUnprocessableEntityException()
        {
            // Arrange
            var creditoDtoInvalido = ObjectMother.GetDtoComValorIssqnInvalido();
            var errosDeValidacao = new List<ValidationFailure>
            {
                new ValidationFailure("ValorIssqn", "O valor do ISSQN deve ser maior que zero.")
            };
            var resultadoValidacaoFalha = new ValidationResult(errosDeValidacao);

            _mockValidator.Setup(v => v.ValidateAsync(creditoDtoInvalido, It.IsAny<System.Threading.CancellationToken>()))
                          .ReturnsAsync(resultadoValidacaoFalha);

            // Act
            var acao = async () => await _creditoService.IntegrarCreditosAsync(new List<CreditoDto> { creditoDtoInvalido });

            // Assert
            acao.Should().ThrowAsync<UnprocessableEntityException>()
                .WithMessage("*O valor do ISSQN deve ser maior que zero.*"); // Verifica se a mensagem de erro está contida na exceção.
            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Credito>()), Times.Never);
        }
    }
}
