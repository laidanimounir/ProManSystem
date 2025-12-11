using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProManSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddProductsAndRecipes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodeProduit = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PrixVente = table.Column<decimal>(type: "TEXT", nullable: false),
                    StockActuel = table.Column<decimal>(type: "TEXT", nullable: false),
                    StockMin = table.Column<decimal>(type: "TEXT", nullable: false),
                    CoutProduction = table.Column<decimal>(type: "TEXT", nullable: false),
                    Marge = table.Column<decimal>(type: "TEXT", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductRecipes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    RawMaterialId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantiteNecessaire = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRecipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductRecipes_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductRecipes_RawMaterials_RawMaterialId",
                        column: x => x.RawMaterialId,
                        principalTable: "RawMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductRecipes_ProductId",
                table: "ProductRecipes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRecipes_RawMaterialId",
                table: "ProductRecipes",
                column: "RawMaterialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductRecipes");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
