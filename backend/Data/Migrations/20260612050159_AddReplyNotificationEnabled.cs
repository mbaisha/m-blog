using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mblog.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReplyNotificationEnabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameColumn(
                name: "LoginCaptchaEnabled",
                table: "site_settings",
                newName: "ReplyNotificationEnabled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReplyNotificationEnabled",
                table: "site_settings",
                newName: "LoginCaptchaEnabled");

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
        }
    }
}
