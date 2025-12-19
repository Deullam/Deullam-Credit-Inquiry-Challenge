using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Deullam.Credit.Inquiry.Challenge.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "credito",
                columns: new[] { "id", "aliquota", "base_calculo", "data_constituicao", "numero_credito", "numero_nfse", "simples_nacional", "tipo_credito", "valor_deducao", "valor_faturado", "valor_issqn" },
                values: new object[,]
                {
                    { 1L, 5.0m, 25000.00m, new DateTime(2024, 2, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "123456", "7891011", true, "ISSQN", 5000.00m, 30000.00m, 1500.75m },
                    { 2L, 4.5m, 21000.00m, new DateTime(2024, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "789012", "7891011", false, "ISSQN", 4000.00m, 25000.00m, 1200.50m },
                    { 3L, 3.5m, 17000.00m, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "654321", "1122334", true, "Outros", 3000.00m, 20000.00m, 800.50m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "credito",
                keyColumn: "id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "credito",
                keyColumn: "id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "credito",
                keyColumn: "id",
                keyValue: 3L);
        }
    }
}
