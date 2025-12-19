using Deullam.Credit.Inquiry.Challenge.API.BackgroundServices;
using Deullam.Credit.Inquiry.Challenge.API.Middlewares;
using Microsoft.EntityFrameworkCore;
using System;

using Deullam.Credit.Inquiry.Challenge.Infra.Data.Contexts;
using Deullam.Credit.Inquiry.Challenge.IoC.Containers;

namespace Deullam.Credit.Inquiry.Challenge.API
{
    public partial class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddApplicationServices();

            // Passamos o 'builder.Configuration' para que o método tenha acesso ao appsettings.json.
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddHostedService<CreditoConsumerService>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            ApplyDatabaseMigrations(app);

            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
                       

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();

            /// --- MÉTODO AUXILIAR PARA APLICAR MIGRAÇÕES ---
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
                        throw;
                    }
                }
            }
        }


    }
}
