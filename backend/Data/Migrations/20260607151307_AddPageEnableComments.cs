using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mblog.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPageEnableComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableComments",
                table: "pages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "FooterConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Copyright = table.Column<string>(type: "text", nullable: true),
                    IcpNumber = table.Column<string>(type: "text", nullable: true),
                    IcpUrl = table.Column<string>(type: "text", nullable: true),
                    PoliceNumber = table.Column<string>(type: "text", nullable: true),
                    PoliceUrl = table.Column<string>(type: "text", nullable: true),
                    SocialLinks = table.Column<string>(type: "text", nullable: false),
                    Blocks = table.Column<string>(type: "text", nullable: false),
                    Style = table.Column<string>(type: "text", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    ContactEmail = table.Column<string>(type: "text", nullable: true),
                    ContactWeChat = table.Column<string>(type: "text", nullable: true),
                    ContactPhone = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FooterConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModuleLayouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PageKey = table.Column<string>(type: "text", nullable: false),
                    ModuleKey = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Config = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleLayouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NavigationItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    OpenInNewTab = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavigationItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "site_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    SiteName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValue: "My Blog"),
                    SiteDescription = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    LogoImageId = table.Column<Guid>(type: "uuid", nullable: true),
                    FaviconImageId = table.Column<Guid>(type: "uuid", nullable: true),
                    CommentModerationEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    VisitRetentionDays = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_site_settings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_site_settings_media_FaviconImageId",
                        column: x => x.FaviconImageId,
                        principalTable: "media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_site_settings_media_LogoImageId",
                        column: x => x.LogoImageId,
                        principalTable: "media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ThemeSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeName = table.Column<string>(type: "text", nullable: false),
                    PrimaryColor = table.Column<string>(type: "text", nullable: false),
                    AccentColor = table.Column<string>(type: "text", nullable: false),
                    BackgroundColor = table.Column<string>(type: "text", nullable: false),
                    TextColor = table.Column<string>(type: "text", nullable: false),
                    LinkColor = table.Column<string>(type: "text", nullable: false),
                    NavbarBackground = table.Column<string>(type: "text", nullable: true),
                    NavbarTextColor = table.Column<string>(type: "text", nullable: true),
                    BorderRadius = table.Column<string>(type: "text", nullable: false),
                    CustomCss = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_site_settings_FaviconImageId",
                table: "site_settings",
                column: "FaviconImageId");

            migrationBuilder.CreateIndex(
                name: "IX_site_settings_LogoImageId",
                table: "site_settings",
                column: "LogoImageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FooterConfigs");

            migrationBuilder.DropTable(
                name: "ModuleLayouts");

            migrationBuilder.DropTable(
                name: "NavigationItems");

            migrationBuilder.DropTable(
                name: "site_settings");

            migrationBuilder.DropTable(
                name: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "EnableComments",
                table: "pages");
        }
    }
}
