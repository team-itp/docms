using Microsoft.EntityFrameworkCore.Migrations;

namespace Docms.Infrastructure.Migrations
{
    public partial class ChangeIndexOfHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Entries_Path",
                table: "Entries");

            migrationBuilder.DropIndex(
                name: "IX_DocumentHistories_Path",
                table: "DocumentHistories");

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "DocumentHistories",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentHistories_Timestamp",
                table: "DocumentHistories",
                column: "Timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DocumentHistories_Timestamp",
                table: "DocumentHistories");

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
                name: "IX_DocumentHistories_Path",
                table: "DocumentHistories",
                column: "Path");
        }
    }
}
