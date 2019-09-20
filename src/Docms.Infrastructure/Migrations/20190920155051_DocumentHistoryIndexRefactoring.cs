using Microsoft.EntityFrameworkCore.Migrations;

namespace Docms.Infrastructure.Migrations
{
    public partial class DocumentHistoryIndexRefactoring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentHistories",
                table: "DocumentHistories");

            migrationBuilder.DropIndex(
                name: "IX_DocumentHistories_Path_Timestamp",
                table: "DocumentHistories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentHistories",
                table: "DocumentHistories",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentHistories_Path",
                table: "DocumentHistories",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentHistories_Timestamp_Path",
                table: "DocumentHistories",
                columns: new[] { "Timestamp", "Path" })
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentHistories",
                table: "DocumentHistories");

            migrationBuilder.DropIndex(
                name: "IX_DocumentHistories_Path",
                table: "DocumentHistories");

            migrationBuilder.DropIndex(
                name: "IX_DocumentHistories_Timestamp_Path",
                table: "DocumentHistories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentHistories",
                table: "DocumentHistories",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentHistories_Path_Timestamp",
                table: "DocumentHistories",
                columns: new[] { "Path", "Timestamp" });
        }
    }
}
