using Confluent.Kafka;
using Deullam.Credit.Inquiry.Challenge.API.BackgroundServices;
using Deullam.Credit.Inquiry.Challenge.API.Middlewares;
using Deullam.Credit.Inquiry.Challenge.Infra.Data.Contexts;
using Deullam.Credit.Inquiry.Challenge.IoC.Containers;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Deullam.Credit.Inquiry.Challenge.API
{
    public partial class Program
    {
        public static void Main(string[] args)
        {


            var builder = WebApplication.CreateBuilder(args);

            // Adiciona serviços de Injeção de Dependência das camadas de Application e Infra
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            // Adiciona o serviço de background que consome do Kafka
            builder.Services.AddHostedService<CreditoConsumerService>();

            // Adiciona e configura os Health Checks
            builder.Services.AddHealthChecks()
                .AddNpgSql(
                    builder.Configuration.GetConnectionString("Postgres")!,
                    name: "PostgreSQL",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "database", "ready" })
                .AddKafka(
                    new ProducerConfig { BootstrapServers = builder.Configuration.GetConnectionString("Kafka")! },
                    name: "Kafka",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "message-broker", "ready" });

            // Adiciona serviços do MVC e Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // 2. --- CONSTRUÇÃO DA APLICAÇÃO ---
            var app = builder.Build();


            // O middleware de exceção deve ser um dos primeiros a ser registrado
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            // Configura o Swagger apenas em ambiente de desenvolvimento
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Mapeia os endpoints de Health Check ANTES dos controllers
            app.MapHealthChecks("/health/self", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHttpsRedirection();
            app.UseAuthorization();

            // Mapeia os controllers como o passo final do roteamento
            app.MapControllers();

            ApplyDatabaseMigrations(app);

            app.Run();


            // --- MÉTODO AUXILIAR PARA APLICAR MIGRAÇÕES ---
            static void ApplyDatabaseMigrations(IApplicationBuilder app)
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    var context = services.GetRequiredService<AppDbContext>();
                    var environment = services.GetRequiredService<IWebHostEnvironment>();

                    try
                    {
                        logger.LogInformation("Verificando migrações pendentes do banco de dados...");

                        if (context.Database.GetPendingMigrations().Any())
                        {
                            if (environment.IsDevelopment())
                            {
                                logger.LogWarning("AMBIENTE DE DESENVOLVIMENTO: Aplicando migrações pendentes automaticamente...");
                                context.Database.Migrate();
                                logger.LogInformation("Migrações aplicadas com sucesso.");
                            }
                            else
                            {
                                logger.LogCritical("AMBIENTE DE PRODUÇÃO: Existem migrações pendentes! A aplicação automática está desativada. Aplique as migrações manualmente.");
                            }
                        }
                        else
                        {
                            logger.LogInformation("O banco de dados já está atualizado.");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Ocorreu um erro crítico ao verificar ou aplicar as migrações do banco de dados.");
                        throw; // Descomente se a falha na migração deve impedir a inicialização.
                    }
                }
            }
        }
    }
}