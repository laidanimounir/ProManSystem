using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProManSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSuppliers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodeFournisseur = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Designation = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Activite = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Adresse = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    NumeroRC = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    MatriculeFiscal = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TypeIdentification = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    NumeroIdentification = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TotalAchats = table.Column<decimal>(type: "TEXT", nullable: true),
                    Dette = table.Column<decimal>(type: "TEXT", nullable: true),
                    EstActif = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Suppliers");
        }
    }
}
