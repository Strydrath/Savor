using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Savor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeSlugHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recipe_slug_history",
                columns: table => new
                {
                    Slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_slug_history", x => x.Slug);
                    table.ForeignKey(
                        name: "FK_recipe_slug_history_recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recipe_slug_history_RecipeId",
                table: "recipe_slug_history",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recipe_slug_history");
        }
    }
}
