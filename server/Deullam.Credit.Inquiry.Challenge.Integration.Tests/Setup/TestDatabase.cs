using System.Diagnostics;
using Deullam.Credit.Inquiry.Challenge.Infra.Data.Contexts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Deullam.Credit.Inquiry.Challenge.Integration.Tests.Setup
{
    /// <summary>
    /// Banco usado pela suíte de integração.
    /// </summary>
    public interface ITestDatabase : IAsyncDisposable
    {
        /// <summary>Descrição legível do provider em uso, impressa no início da suíte.</summary>
        string Description { get; }

        /// <summary>
        /// Valor publicado em <c>ConnectionStrings:DefaultConnection</c>. A API usa essa chave tanto
        /// para o <c>DbContext</c> quanto para o health check de PostgreSQL.
        /// </summary>
        string DefaultConnectionString { get; }

        /// <summary>Registra o <see cref="AppDbContext"/> apontando para este banco.</summary>
        void ConfigureDbContext(IServiceCollection services);

        /// <summary>Cria o schema (e o seed) antes do primeiro teste.</summary>
        void CreateSchema(AppDbContext context);
    }

    /// <summary>
    /// Escolhe o banco da suíte: PostgreSQL real via Testcontainers quando há Docker, SQLite em
    /// arquivo temporário quando não há.
    /// </summary>
    public static class TestDatabaseFactory
    {
        public static async Task<ITestDatabase> CreateAsync()
        {
            if (DockerIsAvailable())
            {
                var postgres = new PostgresTestDatabase();
                await postgres.StartAsync();
                return postgres;
            }

            return new SqliteTestDatabase();
        }

        /// <summary>Equivalente a rodar <c>docker info</c>: só há Testcontainers se o daemon responder.</summary>
        private static bool DockerIsAvailable()
        {
            try
            {
                var startInfo = new ProcessStartInfo("docker", "info")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process is null)
                {
                    return false;
                }

                if (!process.WaitForExit(30_000))
                {
                    process.Kill(entireProcessTree: true);
                    return false;
                }

                return process.ExitCode == 0;
            }
            catch (Exception)
            {
                // docker não instalado / não no PATH.
                return false;
            }
        }
    }

    /// <summary>
    /// PostgreSQL 15 em contêiner descartável. É o caminho preferido: roda as migrations reais do
    /// EF Core, o mesmo provider Npgsql da produção e deixa o health check de PostgreSQL saudável.
    /// </summary>
    public sealed class PostgresTestDatabase : ITestDatabase
    {
        private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("credit_inquiry_tests")
            .WithUsername("test_user")
            .WithPassword("test_pass")
            .Build();

        public string Description => "PostgreSQL 15 (Testcontainers)";

        public string DefaultConnectionString => _container.GetConnectionString();

        public Task StartAsync() => _container.StartAsync();

        public void ConfigureDbContext(IServiceCollection services) =>
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    DefaultConnectionString,
                    npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        public void CreateSchema(AppDbContext context) => context.Database.Migrate();

        public async ValueTask DisposeAsync() => await _container.DisposeAsync();
    }

    /// <summary>
    /// Alternativa usada quando não há Docker na máquina.
    ///
    /// LIMITAÇÃO CONHECIDA: o schema é criado por <c>EnsureCreated</c> a partir do modelo, e não
    /// pelas migrations do EF Core (que são específicas do Npgsql). O health check de PostgreSQL
    /// também fica indisponível, porque não há servidor PostgreSQL. A cobertura de HTTP, validação,
    /// mensageria e persistência continua valendo; a validação das migrations, não.
    /// </summary>
    public sealed class SqliteTestDatabase : ITestDatabase
    {
        private readonly string _file = Path.Combine(
            Path.GetTempPath(),
            $"credit-inquiry-tests-{Guid.NewGuid():N}.db");

        public string Description => "SQLite (Docker indisponível: as migrations do Npgsql não são exercidas)";

        /// <summary>
        /// Endereço fechado de propósito. A API exige uma connection string de PostgreSQL válida para
        /// registrar o health check; sem Docker não há servidor, então o check reporta indisponível.
        /// </summary>
        public string DefaultConnectionString =>
            "Host=localhost;Port=59432;Database=credit_inquiry_tests;Username=test_user;Password=test_pass;Timeout=1;Command Timeout=1";

        public void ConfigureDbContext(IServiceCollection services) =>
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={_file}"));

        public void CreateSchema(AppDbContext context) => context.Database.EnsureCreated();

        public ValueTask DisposeAsync()
        {
            SqliteConnection.ClearAllPools();

            try
            {
                if (File.Exists(_file))
                {
                    File.Delete(_file);
                }
            }
            catch (IOException)
            {
                // arquivo temporário: falhar ao apagar não invalida a suíte.
            }

            return ValueTask.CompletedTask;
        }
    }
}
