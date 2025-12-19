using Deullam.Credit.Inquiry.Challenge.API;
using Deullam.Credit.Inquiry.Challenge.Infra.Data.Contexts;
using Microsoft.AspNetCore.Hosting;
//using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Deullam.Credit.Inquiry.Challenge.Integration.Tests.Setup
{
    public class WebAppFactory : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;

        public WebAppFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // --- INÍCIO DA SOLUÇÃO ---
            // Esta parte do código encontra o assembly do seu projeto de API
            // e garante que o WebApplicationFactory use o arquivo .deps.json correto.
            builder.UseSetting("APPLICATIONNAME", "Deullam.Credit.Inquiry.Challenge.API");
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                var apiAssembly = typeof(Program).Assembly;
                var apiPath = Path.GetDirectoryName(apiAssembly.Location);
                conf.SetBasePath(apiPath);
            });
            // --- FIM DA SOLUÇÃO ---

            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(_connectionString);
                });

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.Migrate();
                }
            });
        }
    }
}

