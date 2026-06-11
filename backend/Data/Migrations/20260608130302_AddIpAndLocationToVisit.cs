using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mblog.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIpAndLocationToVisit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "visits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "visits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BgColor",
                table: "tags",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "visits");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "visits");

            migrationBuilder.DropColumn(
                name: "BgColor",
                table: "tags");
        }
    }
}
