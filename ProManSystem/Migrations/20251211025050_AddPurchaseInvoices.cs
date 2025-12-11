using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProManSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchaseInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NumeroFacture = table.Column<string>(type: "TEXT", nullable: false),
                    SupplierId = table.Column<int>(type: "INTEGER", nullable: false),
                    DateFacture = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MontantHT = table.Column<decimal>(type: "TEXT", nullable: false),
                    TauxTVA = table.Column<decimal>(type: "TEXT", nullable: false),
                    MontantTVA = table.Column<decimal>(type: "TEXT", nullable: false),
                    MontantTTC = table.Column<decimal>(type: "TEXT", nullable: false),
                    MontantPaye = table.Column<decimal>(type: "TEXT", nullable: false),
                    Reste = table.Column<decimal>(type: "TEXT", nullable: false),
                    EstPayee = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoices_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PurchaseInvoiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    RawMaterialId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantite = table.Column<decimal>(type: "TEXT", nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "TEXT", nullable: false),
                    MontantLigne = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceLines_PurchaseInvoices_PurchaseInvoiceId",
                        column: x => x.PurchaseInvoiceId,
                        principalTable: "PurchaseInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceLines_RawMaterials_RawMaterialId",
                        column: x => x.RawMaterialId,
                        principalTable: "RawMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceLines_PurchaseInvoiceId",
                table: "PurchaseInvoiceLines",
                column: "PurchaseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceLines_RawMaterialId",
                table: "PurchaseInvoiceLines",
                column: "RawMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_SupplierId",
                table: "PurchaseInvoices",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseInvoiceLines");

            migrationBuilder.DropTable(
                name: "PurchaseInvoices");
        }
    }
}
