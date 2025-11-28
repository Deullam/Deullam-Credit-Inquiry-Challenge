namespace Deullam.Credit.Inquiry.Challenge.Application.Tests.Features.GerenciarCredito
{
    using AutoMapper;
    using Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito;
    using Deullam.Credit.Inquiry.Challenge.Common.Tests.Features.GerenciarCredito;
    using Deullam.Credit.Inquiry.Challenge.Domain.Features.GerenciarCredito;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [TestFixture]
    public class CreditoServiceTests
    {
        private Mock<ICreditoRepository> _mockRepository;
        private Mock<IMapper> _mockMapper; // 1. Adicionar mock para o IMapper
        private ICreditoService _creditoService;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<ICreditoRepository>();
            _mockMapper = new Mock<IMapper>(); // 2. Inicializar o mock do IMapper

            // 3. Injetar ambos os mocks no serviço
            _creditoService = new CreditoService(_mockRepository.Object, _mockMapper.Object);
        }

        [Test(Description = "Deve retornar lista de DTOs quando a NFS-e existir.")]
        public async Task GetByNfseAsync_DeveRetornarListaDeCreditoDtos_QuandoExistirem()
        {
            // Arrange
            var numeroNfse = "7891011";
            var creditosEntidades = ObjectMother.GetDefaultCreditoList();
            // O serviço agora deve retornar DTOs. Vamos criar um DTO para o Assert.
            var creditoDto = ObjectMother.GetDefaultCreditoDto();

            _mockRepository.Setup(repo => repo.GetByNfseAsync(numeroNfse))
                           .ReturnsAsync(creditosEntidades);

            // Configura o mock do Mapper: Quando for pedido para mapear uma lista de Credito,
            // retorne uma lista contendo nosso DTO de exemplo.
            _mockMapper.Setup(m => m.Map<IEnumerable<CreditoDto>>(creditosEntidades))
                       .Returns(new List<CreditoDto> { creditoDto });

            // Act
            var resultado = await _creditoService.GetByNfseAsync(numeroNfse);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().ContainSingle(); // Verifica se a lista tem 1 item, como configuramos no mock do mapper
            resultado.Should().BeEquivalentTo(new List<CreditoDto> { creditoDto });
            _mockRepository.Verify(repo => repo.GetByNfseAsync(numeroNfse), Times.Once);
        }

        [Test(Description = "Deve retornar DTO quando o número do crédito existir.")]
        public async Task GetByNumeroCreditoAsync_DeveRetornarCreditoDto_QuandoExistir()
        {
            // Arrange
            var creditoEntidade = ObjectMother.GetDefaultCredito();
            var creditoDto = ObjectMother.GetDefaultCreditoDto();
            var numeroCredito = creditoEntidade.NumeroCredito;

            _mockRepository.Setup(repo => repo.GetByNumeroCreditoAsync(numeroCredito))
                           .ReturnsAsync(creditoEntidade);

            // Configura o mock do Mapper para o mapeamento de um único objeto
            _mockMapper.Setup(m => m.Map<CreditoDto>(creditoEntidade))
                       .Returns(creditoDto);

            // Act
            var resultado = await _creditoService.GetByNumeroCreditoAsync(numeroCredito);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().BeEquivalentTo(creditoDto);
            _mockRepository.Verify(repo => repo.GetByNumeroCreditoAsync(numeroCredito), Times.Once);
        }

        [Test(Description = "Deve adicionar o crédito quando ele não existir.")]
        public async Task CreateIfNotExistsAsync_DeveAdicionarCredito_QuandoNaoExistir()
        {
            // Arrange
            var novoCreditoDto = ObjectMother.GetDefaultCreditoDto();
            var creditoEntidade = ObjectMother.GetDefaultCredito();
            var numeroCredito = novoCreditoDto.NumeroCredito;

            _mockRepository.Setup(repo => repo.ExistsByNumeroCreditoAsync(numeroCredito))
                           .ReturnsAsync(false);

            // Configura o mock do Mapper para a conversão de DTO para Entidade
            _mockMapper.Setup(m => m.Map<Credito>(novoCreditoDto))
                       .Returns(creditoEntidade);

            // Act
            await _creditoService.CreateIfNotExistsAsync(novoCreditoDto);

            // Assert
            // Verifica se o repositório foi chamado com a entidade que o mapper retornou
            _mockRepository.Verify(repo => repo.AddAsync(creditoEntidade), Times.Once);
        }

        [Test(Description = "NÃO deve adicionar o crédito quando ele já existir.")]
        public async Task CreateIfNotExistsAsync_NaoDeveAdicionarCredito_QuandoJaExistir()
        {
            // Arrange
            var creditoExistenteDto = ObjectMother.GetDefaultCreditoDto();
            var numeroCredito = creditoExistenteDto.NumeroCredito;

            _mockRepository.Setup(repo => repo.ExistsByNumeroCreditoAsync(numeroCredito))
                           .ReturnsAsync(true);

            // Act
            await _creditoService.CreateIfNotExistsAsync(creditoExistenteDto);

            // Assert
            // Garante que o método AddAsync NUNCA foi chamado.
            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Credito>()), Times.Never);
            // Garante que o Mapper também NUNCA foi chamado, pois a lógica parou antes.
            _mockMapper.Verify(m => m.Map<Credito>(It.IsAny<CreditoDto>()), Times.Never);
        }
    }
}
