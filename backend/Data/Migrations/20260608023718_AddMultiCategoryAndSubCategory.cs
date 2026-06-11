using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mblog.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiCategoryAndSubCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "categories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "article_categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ArticleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_article_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_article_categories_articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_article_categories_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categories_ParentId",
                table: "categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_article_categories_ArticleId_CategoryId",
                table: "article_categories",
                columns: new[] { "ArticleId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_article_categories_CategoryId",
                table: "article_categories",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_categories_categories_ParentId",
                table: "categories",
                column: "ParentId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_categories_categories_ParentId",
                table: "categories");

            migrationBuilder.DropTable(
                name: "article_categories");

            migrationBuilder.DropIndex(
                name: "IX_categories_ParentId",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "categories");
        }
    }
}
