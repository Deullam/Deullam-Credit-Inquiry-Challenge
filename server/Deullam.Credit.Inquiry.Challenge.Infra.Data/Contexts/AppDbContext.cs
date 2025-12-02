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
            });

            base.OnModelCreating(modelBuilder);
        }
    }

}
