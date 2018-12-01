using Microsoft.EntityFrameworkCore.Migrations;

namespace Docms.Infrastructure.Migrations
{
    public partial class AddIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Documents",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "DocumentHistories",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entries_Path",
                table: "Entries",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Path",
                table: "Documents",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentHistories_Path",
                table: "DocumentHistories",
                column: "Path");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Entries_Path",
                table: "Entries");

            migrationBuilder.DropIndex(
                name: "IX_Documents_Path",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_DocumentHistories_Path",
                table: "DocumentHistories");

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Documents",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "DocumentHistories",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
