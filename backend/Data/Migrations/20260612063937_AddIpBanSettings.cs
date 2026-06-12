using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mblog.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIpBanSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IpBanDurationMinutes",
                table: "site_settings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IpBanThreshold",
                table: "site_settings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IpBanWindowSeconds",
                table: "site_settings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpBanDurationMinutes",
                table: "site_settings");

            migrationBuilder.DropColumn(
                name: "IpBanThreshold",
                table: "site_settings");

            migrationBuilder.DropColumn(
                name: "IpBanWindowSeconds",
                table: "site_settings");
        }
    }
}
