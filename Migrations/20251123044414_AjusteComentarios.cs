using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace As.Api.Migrations
{
    /// <inheritdoc />
    public partial class AjusteComentarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Respostas_RespostaId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "RespostaId",
                table: "Comments",
                newName: "EnqueteId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_RespostaId",
                table: "Comments",
                newName: "IX_Comments_EnqueteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Enquetes_EnqueteId",
                table: "Comments",
                column: "EnqueteId",
                principalTable: "Enquetes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Enquetes_EnqueteId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "EnqueteId",
                table: "Comments",
                newName: "RespostaId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_EnqueteId",
                table: "Comments",
                newName: "IX_Comments_RespostaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Respostas_RespostaId",
                table: "Comments",
                column: "RespostaId",
                principalTable: "Respostas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
