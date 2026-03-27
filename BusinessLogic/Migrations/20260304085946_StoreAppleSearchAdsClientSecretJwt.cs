using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groz_Backend.Migrations
{
    /// <inheritdoc />
    public partial class StoreAppleSearchAdsClientSecretJwt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientSecretJwt",
                table: "AppleSearchAdsCredentials",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClientSecretJwtExpiresAt",
                table: "AppleSearchAdsCredentials",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientSecretJwt",
                table: "AppleSearchAdsCredentials");

            migrationBuilder.DropColumn(
                name: "ClientSecretJwtExpiresAt",
                table: "AppleSearchAdsCredentials");
        }
    }
}
