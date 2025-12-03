using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace As.Api.Migrations
{
    /// <inheritdoc />
    public partial class inserindoAcesso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Obrigatoria",
                table: "Perguntas",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ContaParaAcesso",
                table: "Enquetes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserAcessos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    LoginGerado = table.Column<string>(type: "TEXT", nullable: false),
                    SenhaHash = table.Column<string>(type: "TEXT", nullable: false),
                    DataGeracao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAcessos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAcessos_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAcessos_UserId",
                table: "UserAcessos",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAcessos");

            migrationBuilder.DropColumn(
                name: "Obrigatoria",
                table: "Perguntas");

            migrationBuilder.DropColumn(
                name: "ContaParaAcesso",
                table: "Enquetes");
        }
    }
}
