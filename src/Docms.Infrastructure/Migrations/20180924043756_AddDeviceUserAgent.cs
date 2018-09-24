using Microsoft.EntityFrameworkCore.Migrations;

namespace Docms.Infrastructure.Migrations
{
    public partial class AddDeviceUserAgent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceUserAgent",
                table: "Devices",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceUserAgent",
                table: "DeviceGrants",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceUserAgent",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "DeviceUserAgent",
                table: "DeviceGrants");
        }
    }
}
