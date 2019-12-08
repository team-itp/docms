using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Docms.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
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

            migrationBuilder.CreateTable(
                name: "DeviceGrants",
                columns: table => new
                {
                    DeviceId = table.Column<string>(nullable: false),
                    DeviceUserAgent = table.Column<string>(nullable: true),
                    IsGranted = table.Column<bool>(nullable: false),
                    GrantedBy = table.Column<string>(nullable: true),
                    GrantedAt = table.Column<DateTime>(nullable: true),
                    LastAccessUserId = table.Column<string>(nullable: true),
                    LastAccessUserName = table.Column<string>(nullable: true),
                    LastAccessTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceGrants", x => x.DeviceId);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DeviceId = table.Column<string>(nullable: true),
                    UsedBy = table.Column<string>(nullable: true),
                    Granted = table.Column<bool>(nullable: false),
                    DeviceUserAgent = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    DocumentId = table.Column<int>(nullable: false),
                    Path = table.Column<string>(maxLength: 800, nullable: false),
                    StorageKey = table.Column<string>(nullable: true),
                    ContentType = table.Column<string>(nullable: true),
                    FileSize = table.Column<long>(nullable: true),
                    Hash = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: true),
                    LastModified = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentHistories", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Path = table.Column<string>(maxLength: 800, nullable: true),
                    ContentType = table.Column<string>(nullable: true),
                    FileSize = table.Column<long>(nullable: false),
                    Hash = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    LastModified = table.Column<DateTime>(nullable: false),
                    StorageKey = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    Path = table.Column<string>(maxLength: 800, nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ParentPath = table.Column<string>(maxLength: 800, nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    DocumentId = table.Column<int>(nullable: true),
                    StorageKey = table.Column<string>(nullable: true),
                    ContentType = table.Column<string>(nullable: true),
                    FileSize = table.Column<long>(nullable: true),
                    Hash = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: true),
                    LastModified = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.Path);
                    table.ForeignKey(
                        name: "FK_Entries_Entries_ParentPath",
                        column: x => x.ParentPath,
                        principalTable: "Entries",
                        principalColumn: "Path",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    Role = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.Role });
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    SecurityStamp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentHistories_Path",
                table: "DocumentHistories",
                column: "Path");

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
                name: "IX_DocumentHistories_Timestamp_Id",
                table: "DocumentHistories",
                columns: new[] { "Timestamp", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentHistories_Timestamp_Path",
                table: "DocumentHistories",
                columns: new[] { "Timestamp", "Path" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Path",
                table: "Documents",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_ParentPath",
                table: "Entries",
                column: "ParentPath");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientInfo");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "DeviceGrants");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "DocumentHistories");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Entries");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
