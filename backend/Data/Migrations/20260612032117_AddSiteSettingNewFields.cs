using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mblog.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteSettingNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CommentReplyNotificationEnabled",
                table: "site_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IpBlockDurationMinutes",
                table: "site_settings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IpRateLimitThreshold",
                table: "site_settings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IpRateLimitWindowMinutes",
                table: "site_settings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "LoginCaptchaEnabled",
                table: "site_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentReplyNotificationEnabled",
                table: "site_settings");

            migrationBuilder.DropColumn(
                name: "IpBlockDurationMinutes",
                table: "site_settings");

            migrationBuilder.DropColumn(
                name: "IpRateLimitThreshold",
                table: "site_settings");

            migrationBuilder.DropColumn(
                name: "IpRateLimitWindowMinutes",
                table: "site_settings");

            migrationBuilder.DropColumn(
                name: "LoginCaptchaEnabled",
                table: "site_settings");
        }
    }
}
