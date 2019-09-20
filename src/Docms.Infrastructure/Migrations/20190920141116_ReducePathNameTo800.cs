using Microsoft.EntityFrameworkCore.Migrations;

namespace Docms.Infrastructure.Migrations
{
    public partial class ReducePathNameTo800 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ParentPath",
                table: "Entries",
                maxLength: 800,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Entries",
                maxLength: 800,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Documents",
                maxLength: 800,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "DocumentHistories",
                maxLength: 800,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 4000);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ParentPath",
                table: "Entries",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 800,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Entries",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 800);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Documents",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 800,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "DocumentHistories",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 800);
        }
    }
}
