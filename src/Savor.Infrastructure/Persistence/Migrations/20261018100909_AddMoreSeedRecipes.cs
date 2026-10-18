using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Savor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreSeedRecipes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "recipes",
                columns: new[] { "Id", "Description", "OwnerId", "Title", "Visibility" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333333"), "A fast stir-fry with chicken, garlic, and vegetables.", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Garlic Chicken Stir-Fry", "Public" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "A fluffy omelette with eggs, tomato, and onion.", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Garden Omelette", "Public" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Golden bread with garlic and butter.", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Garlic Bread", "Public" }
                });

            migrationBuilder.InsertData(
                table: "recipe_ingredients",
                columns: new[] { "Name", "RecipeId", "Quantity", "Unit" },
                values: new object[,]
                {
                    { "chicken", new Guid("33333333-3333-3333-3333-333333333333"), 300m, "g" },
                    { "garlic", new Guid("33333333-3333-3333-3333-333333333333"), 2m, "cloves" },
                    { "onion", new Guid("33333333-3333-3333-3333-333333333333"), 1m, "piece" },
                    { "egg", new Guid("44444444-4444-4444-4444-444444444444"), 3m, "pieces" },
                    { "onion", new Guid("44444444-4444-4444-4444-444444444444"), 0.5m, "piece" },
                    { "tomato", new Guid("44444444-4444-4444-4444-444444444444"), 1m, "piece" },
                    { "bread", new Guid("55555555-5555-5555-5555-555555555555"), 1m, "loaf" },
                    { "butter", new Guid("55555555-5555-5555-5555-555555555555"), 50m, "g" },
                    { "garlic", new Guid("55555555-5555-5555-5555-555555555555"), 3m, "cloves" }
                });

            migrationBuilder.InsertData(
                table: "recipe_steps",
                columns: new[] { "RecipeId", "StepNumber", "Instruction" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333333"), 1, "Stir-fry the chicken until cooked." },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 2, "Add the garlic and onion, then serve." },
                    { new Guid("44444444-4444-4444-4444-444444444444"), 1, "Whisk the eggs and cook with the vegetables." },
                    { new Guid("55555555-5555-5555-5555-555555555555"), 1, "Spread garlic butter on the bread and bake." }
                });

            migrationBuilder.InsertData(
                table: "recipe_tags",
                columns: new[] { "Name", "RecipeId" },
                values: new object[,]
                {
                    { "quick", new Guid("33333333-3333-3333-3333-333333333333") },
                    { "breakfast", new Guid("44444444-4444-4444-4444-444444444444") },
                    { "side", new Guid("55555555-5555-5555-5555-555555555555") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "recipe_ingredients",
                keyColumns: new[] { "Name", "RecipeId" },
                keyValues: new object[] { "chicken", new Guid("33333333-3333-3333-3333-333333333333") });

            migrationBuilder.DeleteData(
                table: "recipe_ingredients",
                keyColumns: new[] { "Name", "RecipeId" },
                keyValues: new object[] { "garlic", new Guid("33333333-3333-3333-3333-333333333333") });

            migrationBuilder.DeleteData(
                table: "recipe_ingredients",
                keyColumns: new[] { "Name", "RecipeId" },
                keyValues: new object[] { "onion", new Guid("33333333-3333-3333-3333-333333333333") });

            migrationBuilder.DeleteData(
                table: "recipe_ingredients",
                keyColumns: new[] { "Name", "RecipeId" },
                keyValues: new object[] { "egg", new Guid("44444444-4444-4444-4444-444444444444") });

            migrationBuilder.DeleteData(
                table: "recipe_ingredients",
                keyColumns: new[] { "Name", "RecipeId" },
                keyValues: new object[] { "onion", new Guid("44444444-4444-4444-4444-444444444444") });

            migrationBuilder.DeleteData(
                table: "recipe_ingredients",
                keyColumns: new[] { "Name", "RecipeId" },
                keyValues: new object[] { "tomato", new Guid("44444444-4444-4444-4444-444444444444") });

            migrationBuilder.DeleteData(
                table: "recipe_ingredients",
                keyColumns: new[] { "Name", "RecipeId" },
                keyValues: new object[] { "bread", new Guid("55555555-5555-5555-5555-555555555555") });

            migrationBuilder.DeleteData(
                table: "recipe_ingredients",
                keyColumns: new[] { "Name", "RecipeId" },
                keyValues: new object[] { "butter", new Guid("55555555-5555-5555-5555-555555555555") });

            migrationBuilder.DeleteData(
                table: "recipe_ingredients",
                keyColumns: new[] { "Name", "RecipeId" },
                keyValues: new object[] { "garlic", new Guid("55555555-5555-5555-5555-555555555555") });

            migrationBuilder.DeleteData(
                table: "recipe_steps",
                keyColumns: new[] { "RecipeId", "StepNumber" },
                keyValues: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), 1 });

            migrationBuilder.DeleteData(
                table: "recipe_steps",
                keyColumns: new[] { "RecipeId", "StepNumber" },
                keyValues: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), 2 });

            migrationBuilder.DeleteData(
                table: "recipe_steps",
                keyColumns: new[] { "RecipeId", "StepNumber" },
                keyValues: new object[] { new Guid("44444444-4444-4444-4444-444444444444"), 1 });

            migrationBuilder.DeleteData(
                table: "recipe_steps",
                keyColumns: new[] { "RecipeId", "StepNumber" },
                keyValues: new object[] { new Guid("55555555-5555-5555-5555-555555555555"), 1 });

            migrationBuilder.DeleteData(
                table: "recipe_tags",
                keyColumns: new[] { "Name", "RecipeId" },
                keyValues: new object[] { "quick", new Guid("33333333-3333-3333-3333-333333333333") });

            migrationBuilder.DeleteData(
                table: "recipe_tags",
                keyColumns: new[] { "Name", "RecipeId" },
                keyValues: new object[] { "breakfast", new Guid("44444444-4444-4444-4444-444444444444") });

            migrationBuilder.DeleteData(
                table: "recipe_tags",
                keyColumns: new[] { "Name", "RecipeId" },
                keyValues: new object[] { "side", new Guid("55555555-5555-5555-5555-555555555555") });

            migrationBuilder.DeleteData(
                table: "recipes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "recipes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "recipes",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));
        }
    }
}
