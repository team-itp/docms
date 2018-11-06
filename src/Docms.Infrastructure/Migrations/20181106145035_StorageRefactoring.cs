using Microsoft.EntityFrameworkCore.Migrations;

namespace Docms.Infrastructure.Migrations
{
    public partial class StorageRefactoring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StorageKey",
                table: "Entries",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageKey",
                table: "Documents",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorageKey",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "StorageKey",
                table: "Documents");
        }
    }
}
