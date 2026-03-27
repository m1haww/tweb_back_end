using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groz_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedAppUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppId = table.Column<string>(type: "text", nullable: false),
                    TotalRevenue = table.Column<double>(type: "double precision", nullable: false),
                    InstallDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CampaignId = table.Column<long>(type: "bigint", nullable: true),
                    KeywordId = table.Column<long>(type: "bigint", nullable: true),
                    AdGroupId = table.Column<long>(type: "bigint", nullable: true),
                    CountryCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUsers");
        }
    }
}
