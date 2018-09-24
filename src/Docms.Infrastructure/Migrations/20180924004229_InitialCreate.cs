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
                name: "DeviceGrants",
                columns: table => new
                {
                    DeviceId = table.Column<string>(nullable: false),
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
                    Path = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    LastModified = table.Column<DateTime>(nullable: true),
                    ContentType = table.Column<string>(nullable: true),
                    FileSize = table.Column<long>(nullable: true),
                    Hash = table.Column<string>(nullable: true),
                    OldPath = table.Column<string>(nullable: true),
                    NewPath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Path = table.Column<string>(nullable: true),
                    ContentType = table.Column<string>(nullable: true),
                    FileSize = table.Column<long>(nullable: false),
                    Hash = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    LastModified = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    Path = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ParentPath = table.Column<string>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    ContentType = table.Column<string>(nullable: true),
                    FileSize = table.Column<long>(nullable: true),
                    Hash = table.Column<string>(nullable: true),
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
                name: "IX_Entries_ParentPath",
                table: "Entries",
                column: "ParentPath");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "Users");
        }
    }
}
