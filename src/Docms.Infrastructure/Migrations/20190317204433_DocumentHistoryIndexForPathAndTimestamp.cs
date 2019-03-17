using Microsoft.EntityFrameworkCore.Migrations;

namespace Docms.Infrastructure.Migrations
{
    public partial class DocumentHistoryIndexForPathAndTimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DocumentHistories_Timestamp",
                table: "DocumentHistories");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentHistories_Path_Timestamp",
                table: "DocumentHistories",
                columns: new[] { "Path", "Timestamp" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DocumentHistories_Path_Timestamp",
                table: "DocumentHistories");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentHistories_Timestamp",
                table: "DocumentHistories",
                column: "Timestamp");
        }
    }
}
