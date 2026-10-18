using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Savor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeSlugs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "recipes",
                type: "character varying(220)",
                maxLength: 220,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "recipes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "Slug",
                value: "creamy-tomato-pasta");

            migrationBuilder.UpdateData(
                table: "recipes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "Slug",
                value: "family-sunday-stew");

            migrationBuilder.UpdateData(
                table: "recipes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "Slug",
                value: "garlic-chicken-stir-fry");

            migrationBuilder.UpdateData(
                table: "recipes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "Slug",
                value: "garden-omelette");

            migrationBuilder.UpdateData(
                table: "recipes",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "Slug",
                value: "garlic-bread");

            migrationBuilder.CreateIndex(
                name: "IX_recipes_Slug",
                table: "recipes",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_recipes_Slug",
                table: "recipes");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "recipes");
        }
    }
}
