using Deullam.Credit.Inquiry.Challenge.Domain.Features.GerenciarCredito;
using Microsoft.EntityFrameworkCore;

namespace Deullam.Credit.Inquiry.Challenge.Infra.Data.Contexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Declara que a entidade 'Credito' deve ser mapeada para uma tabela no banco de dados.
        public DbSet<Credito> Creditos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aqui configuramos o mapeamento da entidade para a tabela usando a Fluent API.
            // Isso nos dá mais controle do que usar Data Annotations na entidade.
            modelBuilder.Entity<Credito>(entity =>
            {
                // Define o nome da tabela como 'credito' (em minúsculas, como no seu script SQL).
                entity.ToTable("credito");

                // Define a chave primária.
                entity.HasKey(e => e.Id);

                // Mapeia cada propriedade para uma coluna, especificando o tipo e se é obrigatória.
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.NumeroCredito)
                    .HasColumnName("numero_credito")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.NumeroNfse)
                    .HasColumnName("numero_nfse")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.DataConstituicao)
                    .HasColumnName("data_constituicao")
                    .HasColumnType("DATE")
                    .IsRequired();

                entity.Property(e => e.ValorIssqn)
                    .HasColumnName("valor_issqn")
                    .HasColumnType("DECIMAL(15, 2)")
                    .IsRequired();

                entity.Property(e => e.TipoCredito)
                    .HasColumnName("tipo_credito")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.SimplesNacional)
                    .HasColumnName("simples_nacional")
                    .IsRequired();

                entity.Property(e => e.Aliquota)
                    .HasColumnName("aliquota")
                    .HasColumnType("DECIMAL(5, 2)")
                    .IsRequired();

                entity.Property(e => e.ValorFaturado)
                    .HasColumnName("valor_faturado")
                    .HasColumnType("DECIMAL(15, 2)")
                    .IsRequired();

                entity.Property(e => e.ValorDeducao)
                    .HasColumnName("valor_deducao")
                    .HasColumnType("DECIMAL(15, 2)")
                    .IsRequired();

                entity.Property(e => e.BaseCalculo)
                    .HasColumnName("base_calculo")
                    .HasColumnType("DECIMAL(15, 2)")
                    .IsRequired();


                entity.HasData(
                   new Credito
                   {
                       Id = 1, // É importante definir o ID manualmente para o seed.
                       NumeroCredito = "123456",
                       NumeroNfse = "7891011",
                       DataConstituicao = new System.DateTime(2024, 2, 25),
                       ValorIssqn = 1500.75m,
                       TipoCredito = "ISSQN",
                       SimplesNacional = true, // bool para 'Sim'
                       Aliquota = 5.0m,
                       ValorFaturado = 30000.00m,
                       ValorDeducao = 5000.00m,
                       BaseCalculo = 25000.00m
                   },
                   new Credito
                   {
                       Id = 2,
                       NumeroCredito = "789012",
                       NumeroNfse = "7891011",
                       DataConstituicao = new System.DateTime(2024, 2, 26),
                       ValorIssqn = 1200.50m,
                       TipoCredito = "ISSQN",
                       SimplesNacional = false, // bool para 'Não'
                       Aliquota = 4.5m,
                       ValorFaturado = 25000.00m,
                       ValorDeducao = 4000.00m,
                       BaseCalculo = 21000.00m
                   },
                   new Credito
                   {
                       Id = 3,
                       NumeroCredito = "654321",
                       NumeroNfse = "1122334",
                       DataConstituicao = new System.DateTime(2024, 1, 15),
                       ValorIssqn = 800.50m,
                       TipoCredito = "Outros",
                       SimplesNacional = true, // bool para 'Sim'
                       Aliquota = 3.5m,
                       ValorFaturado = 20000.00m,
                       ValorDeducao = 3000.00m,
                       BaseCalculo = 17000.00m
                   }
               );
                // --- FIM DO BLOCO DE SEED DE DADOS ---


            });



            base.OnModelCreating(modelBuilder);
        }
    }

}
