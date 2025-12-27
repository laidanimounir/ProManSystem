using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProManSystem.Migrations
{
  
    public partial class AddRemiseAndModeReglementToSalesInvoice : Migration
    {
       
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "ModeReglement",
                table: "SalesInvoices",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "NetHT",
                table: "SalesInvoices",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RemiseMontant",
                table: "SalesInvoices",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RemiseValeur",
                table: "SalesInvoices",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TypeRemise",
                table: "SalesInvoices",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModeReglement",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "NetHT",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "RemiseMontant",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "RemiseValeur",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "TypeRemise",
                table: "SalesInvoices");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Products",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }
    }
}
