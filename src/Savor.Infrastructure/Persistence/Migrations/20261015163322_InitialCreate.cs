using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Savor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recipe_ingredients",
                columns: table => new
                {
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_ingredients", x => new { x.RecipeId, x.Name });
                    table.ForeignKey(
                        name: "FK_recipe_ingredients_recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipe_tags",
                columns: table => new
                {
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_tags", x => new { x.RecipeId, x.Name });
                    table.ForeignKey(
                        name: "FK_recipe_tags_recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "recipes",
                columns: new[] { "Id", "Description", "OwnerId", "Title", "Visibility" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "A quick pasta with tomato, garlic, and cream.", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Creamy Tomato Pasta", "Public" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "A private recipe used to verify visibility filtering.", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Family Sunday Stew", "Private" }
                });

            migrationBuilder.InsertData(
                table: "recipe_ingredients",
                columns: new[] { "Name", "RecipeId", "Quantity", "Unit" },
                values: new object[,]
                {
                    { "garlic", new Guid("11111111-1111-1111-1111-111111111111"), 2m, "cloves" },
                    { "pasta", new Guid("11111111-1111-1111-1111-111111111111"), 250m, "g" },
                    { "tomato", new Guid("11111111-1111-1111-1111-111111111111"), 400m, "g" },
                    { "beef", new Guid("22222222-2222-2222-2222-222222222222"), 500m, "g" }
                });

            migrationBuilder.InsertData(
                table: "recipe_tags",
                columns: new[] { "Name", "RecipeId" },
                values: new object[,]
                {
                    { "pasta", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "quick", new Guid("11111111-1111-1111-1111-111111111111") },
                    { "family", new Guid("22222222-2222-2222-2222-222222222222") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recipe_ingredients");

            migrationBuilder.DropTable(
                name: "recipe_tags");

            migrationBuilder.DropTable(
                name: "recipes");
        }
    }
}
