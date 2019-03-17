using Microsoft.EntityFrameworkCore.Migrations;

namespace Docms.Infrastructure.Migrations
{
    public partial class RemoveMoveHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OldPath",
                table: "DocumentHistories");

            migrationBuilder.RenameColumn(
                name: "NewPath",
                table: "DocumentHistories",
                newName: "StorageKey");

            migrationBuilder.Sql("update T1 " 
                + " set T1.StorageKey = T2.StorageKey"
                + " from DocumentHistories T1"
                + "      left join Documents T2"
                + "        on T1.Path = T2.Path"
                + "       and T1.Timestamp = (select max(Timestamp) from DocumentHistories T3 where T3.Path = T1.Path)"
                + " where T1.Discriminator in ('DocumentCreated', 'DocumentUpdated')"
                + " and T1.StorageKey is null");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StorageKey",
                table: "DocumentHistories",
                newName: "NewPath");

            migrationBuilder.AddColumn<string>(
                name: "OldPath",
                table: "DocumentHistories",
                nullable: true);

            migrationBuilder.Sql("update DocumentHistories set StorageKey = null");
        }
    }
}
