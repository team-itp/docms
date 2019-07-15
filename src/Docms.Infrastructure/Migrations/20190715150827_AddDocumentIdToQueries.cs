using Microsoft.EntityFrameworkCore.Migrations;

namespace Docms.Infrastructure.Migrations
{
    public partial class AddDocumentIdToQueries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentId",
                table: "Entries",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocumentId",
                table: "DocumentHistories",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("update T1 "
                + " set T1.DocumentId = T2.Id"
                + " from DocumentHistories T1"
                + "      inner join Documents T2"
                + "        on T1.Path = T2.Path");

            migrationBuilder.Sql("update T1 "
                + " set T1.DocumentId = T2.Id"
                + " from Entries T1"
                + "      inner join Documents T2"
                + "        on T1.Path = T2.Path");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "DocumentHistories");
        }
    }
}
