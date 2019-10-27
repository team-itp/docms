using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Docms.Infrastructure.Migrations
{
    public partial class AddClientTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientInfo",
                columns: table => new
                {
                    ClientId = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    IpAddress = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    LastAccessedTime = table.Column<DateTime>(nullable: true),
                    RequestId = table.Column<string>(nullable: true),
                    RequestType = table.Column<string>(nullable: true),
                    RequestedAt = table.Column<DateTime>(nullable: true),
                    AcceptedRequestId = table.Column<string>(nullable: true),
                    AcceptedRequestType = table.Column<string>(nullable: true),
                    AcceptedAt = table.Column<DateTime>(nullable: true),
                    LastMessage = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientInfo", x => x.ClientId);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClientId = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    IpAddress = table.Column<string>(nullable: true),
                    RequestId = table.Column<string>(nullable: true),
                    RequestType = table.Column<string>(nullable: true),
                    IsAccepted = table.Column<bool>(nullable: false),
                    LastAccessedTime = table.Column<DateTime>(nullable: true),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientInfo");

            migrationBuilder.DropTable(
                name: "Clients");
        }
    }
}
