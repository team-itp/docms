using Microsoft.EntityFrameworkCore.Migrations;

namespace Docms.Infrastructure.Migrations
{
    public partial class ChangeIndexesOfHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DocumentHistories_Timestamp_Path",
                table: "DocumentHistories");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentHistories_Timestamp",
                table: "DocumentHistories",
                column: "Timestamp")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentHistories_Path_Timestamp",
                table: "DocumentHistories",
                columns: new[] { "Path", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentHistories_Timestamp_Path",
                table: "DocumentHistories",
                columns: new[] { "Timestamp", "Path" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DocumentHistories_Timestamp",
                table: "DocumentHistories");

            migrationBuilder.DropIndex(
                name: "IX_DocumentHistories_Path_Timestamp",
                table: "DocumentHistories");

            migrationBuilder.DropIndex(
                name: "IX_DocumentHistories_Timestamp_Path",
                table: "DocumentHistories");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentHistories_Timestamp_Path",
                table: "DocumentHistories",
                columns: new[] { "Timestamp", "Path" })
                .Annotation("SqlServer:Clustered", true);
        }
    }
}
