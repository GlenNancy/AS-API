using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace As.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTextoProvocativoPergunta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TextoProvocativo",
                table: "Perguntas",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TextoProvocativo",
                table: "Perguntas");
        }
    }
}
