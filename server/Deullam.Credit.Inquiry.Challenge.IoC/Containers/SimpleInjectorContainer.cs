using Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito;
using Deullam.Credit.Inquiry.Challenge.Application.Features.GerenciarCredito.Validators;
using Deullam.Credit.Inquiry.Challenge.Application.Mappers;
using Deullam.Credit.Inquiry.Challenge.Application.Messaging;
using Deullam.Credit.Inquiry.Challenge.Domain.Features.GerenciarCredito;
using Deullam.Credit.Inquiry.Challenge.Infra.Messaging;
using Deullam.Credit.Inquiry.Challenge.Infra.Data.Contexts;
using Deullam.Credit.Inquiry.Challenge.Infra.Data.Features.GerenciarCredito;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Deullam.Credit.Inquiry.Challenge.IoC.Containers
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile).Assembly);
            services.AddScoped<ICreditoService, CreditoService>();
            services.AddScoped<IValidator<CreditoDto>, CreditoDtoValidator>();
            services.AddSingleton<IMessagePublisher, KafkaPublisher>();

            return services;
        }

        /// <summary>
        /// Registra os serviços da camada de Infraestrutura, como o DbContext e os Repositórios.
        /// Lê a string de conexão do appsettings.json (ou do User Secrets / .env).
        /// O nome "DefaultConnection" deve corresponder ao que está no seu arquivo de configuração.
        /// </summary>
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptionsAction: sqlOptions =>
                    {
                        // Define o nome do assembly onde as migrations estão localizadas.
                        sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    }
                )
            );

            services.AddScoped<ICreditoRepository, CreditoRepository>();

            return services;
        }
    }
}
