using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mblog.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreThemeStyleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeBackground",
                table: "ThemeSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DangerColor",
                table: "ThemeSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FontFamily",
                table: "ThemeSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterBackground",
                table: "ThemeSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterTextColor",
                table: "ThemeSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroBackground",
                table: "ThemeSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuccessColor",
                table: "ThemeSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarningColor",
                table: "ThemeSettings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeBackground",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "DangerColor",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "FontFamily",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "FooterBackground",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "FooterTextColor",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "HeroBackground",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "SuccessColor",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "WarningColor",
                table: "ThemeSettings");
        }
    }
}
