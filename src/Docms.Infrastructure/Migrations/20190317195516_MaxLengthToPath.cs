using Microsoft.EntityFrameworkCore.Migrations;

namespace Docms.Infrastructure.Migrations
{
    public partial class MaxLengthToPath : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_Entries_ParentPath",
                table: "Entries");

            migrationBuilder.AlterColumn<string>(
                name: "ParentPath",
                table: "Entries",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Entries",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Documents",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "DocumentHistories",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_Entries_ParentPath",
                table: "Entries",
                column: "ParentPath",
                principalTable: "Entries",
                principalColumn: "Path",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_Entries_ParentPath",
                table: "Entries");

            migrationBuilder.AlterColumn<string>(
                name: "ParentPath",
                table: "Entries",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Entries",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Documents",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "DocumentHistories",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 4000);

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_Entries_ParentPath",
                table: "Entries",
                column: "ParentPath",
                principalTable: "Entries",
                principalColumn: "Path",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
