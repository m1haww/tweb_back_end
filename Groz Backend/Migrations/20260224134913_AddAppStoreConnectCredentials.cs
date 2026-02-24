using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groz_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAppStoreConnectCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppStoreConnectCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssuerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    KeyId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PrivateKey = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStoreConnectCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStoreConnectCredentials_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppStoreConnectCredentials_UserId",
                table: "AppStoreConnectCredentials",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppStoreConnectCredentials");
        }
    }
}
