using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mblog.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeoutAndAvailableSizes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeoutSeconds",
                table: "LlmConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 300);

            migrationBuilder.AddColumn<string>(
                name: "AvailableSizes",
                table: "ImageGenConfigs",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "TimeoutSeconds",
                table: "ImageGenConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 300);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeoutSeconds",
                table: "LlmConfigs");

            migrationBuilder.DropColumn(
                name: "AvailableSizes",
                table: "ImageGenConfigs");

            migrationBuilder.DropColumn(
                name: "TimeoutSeconds",
                table: "ImageGenConfigs");
        }
    }
}
