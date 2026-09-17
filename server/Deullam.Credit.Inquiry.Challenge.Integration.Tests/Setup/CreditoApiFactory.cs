using Deullam.Credit.Inquiry.Challenge.API;
using Deullam.Credit.Inquiry.Challenge.API.BackgroundServices;
using Deullam.Credit.Inquiry.Challenge.Application.Messaging;
using Deullam.Credit.Inquiry.Challenge.Infra.Data.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Deullam.Credit.Inquiry.Challenge.Integration.Tests.Setup
{
    /// <summary>
    /// Sobe a API inteira em memória (<see cref="WebApplicationFactory{TEntryPoint}"/>) com dois
    /// dublês, e só dois:
    ///
    /// <list type="bullet">
    ///   <item>o produtor Kafka (<see cref="IMessagePublisher"/>) vira <see cref="FakeMessagePublisher"/>;</item>
    ///   <item>o <see cref="CreditoConsumerService"/> não é iniciado, porque não há broker para ele escutar.</item>
    /// </list>
    ///
    /// Todo o resto - pipeline HTTP, middleware de exceção, controllers, AutoMapper,
    /// FluentValidation, repositórios e EF Core - é o código de produção, sem alteração.
    /// </summary>
    public sealed class CreditoApiFactory : WebApplicationFactory<Program>
    {
        private readonly ITestDatabase _database;

        public CreditoApiFactory(ITestDatabase database) => _database = database;

        /// <summary>Mensagens que a API publicaria no Kafka.</summary>
        public FakeMessagePublisher PublishedMessages { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(AppContext.BaseDirectory);

            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _database.DefaultConnectionString,

                    // Endereço fechado de propósito: nenhum teste fala com um broker de verdade.
                    ["ConnectionStrings:Kafka"] = "localhost:59092"
                }));

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IMessagePublisher>();
                services.AddSingleton<IMessagePublisher>(PublishedMessages);

                RemoveKafkaConsumer(services);
                ReplaceDatabase(services);

                // O health check de Kafka usa um producer com timeout longo. Sem broker no host de
                // teste, sem esse limite a chamada a /health/ready ficaria minutos pendurada.
                services.Configure<HealthCheckServiceOptions>(options =>
                {
                    foreach (var registration in options.Registrations)
                    {
                        registration.Timeout = TimeSpan.FromSeconds(5);
                    }
                });
            });
        }

        private static void RemoveKafkaConsumer(IServiceCollection services)
        {
            var consumer = services
                .Where(descriptor => descriptor.ServiceType == typeof(IHostedService)
                                     && descriptor.ImplementationType == typeof(CreditoConsumerService))
                .ToList();

            foreach (var descriptor in consumer)
            {
                services.Remove(descriptor);
            }
        }

        private void ReplaceDatabase(IServiceCollection services)
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<AppDbContext>();

            _database.ConfigureDbContext(services);
        }
    }
}
