using Deullam.Credit.Inquiry.Challenge.Application.Mappers;
using Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Deullam.Credit.Inquiry.Challenge.IoC.Containers
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        /// <summary>
        /// Registra os serviços da camada de Aplicação.
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Registra o AutoMapper, procurando por todos os perfis (como o MappingProfile)
            // no assembly onde a classe MappingProfile está.
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // Registra a dependência do serviço de crédito.
            // Quando alguém pedir um ICreditoService, o contêiner de DI fornecerá uma instância de CreditoService.
            // AddScoped significa que uma nova instância será criada para cada requisição HTTP.
            services.AddScoped<ICreditoService, CreditoService>();

            return services;
        }

        /// <summary>
        /// Registra os serviços da camada de Infraestrutura.
        /// (Este método será preenchido na próxima fase)
        /// </summary>
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Exemplo de como será na próxima fase:
            // services.AddScoped<ICreditoRepository, CreditoRepository>();

            // Adicionar o DbContext aqui também...

            return services;
        }
    }
}
