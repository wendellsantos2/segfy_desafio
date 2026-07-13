using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sinistros.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Apolices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ramo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_apolices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clientes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Sinistros",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    apolice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_ocorrencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_abertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    valor_estimado_quantia = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_estimado_moeda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    valor_aprovado_quantia = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    valor_aprovado_moeda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    motivo_negativa_texto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_encerramento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sinistros", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoSinistros",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sinistro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status_anterior = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    status_novo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_alteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_historico_sinistros", x => x.id);
                    table.ForeignKey(
                        name: "fk_historico_sinistros_sinistros_sinistro_id",
                        column: x => x.sinistro_id,
                        principalTable: "Sinistros",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Apolices_valor",
                table: "Apolices",
                column: "valor",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_historico_sinistros_sinistro_id",
                table: "HistoricoSinistros",
                column: "sinistro_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Apolices");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "HistoricoSinistros");

            migrationBuilder.DropTable(
                name: "Sinistros");
        }
    }
}
