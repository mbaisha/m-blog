using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mblog.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixupThemeActivation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 修复已有数据：先全部设为未激活，再激活最早创建的那个主题
            migrationBuilder.Sql(@"
                UPDATE ""ThemeSettings"" SET ""IsActive"" = false;
                UPDATE ""ThemeSettings"" SET ""IsActive"" = true
                WHERE ""Id"" = (SELECT ""Id"" FROM ""ThemeSettings"" ORDER BY ""CreatedAt"" ASC LIMIT 1);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
