using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mblog.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddThemeSurfaceColorFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ThemeSetting 新增字段
            migrationBuilder.AddColumn<string>(
                name: "BorderColor",
                table: "ThemeSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurfaceColor",
                table: "ThemeSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextSecondaryColor",
                table: "ThemeSettings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BorderColor",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "SurfaceColor",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "TextSecondaryColor",
                table: "ThemeSettings");
        }
    }
}
