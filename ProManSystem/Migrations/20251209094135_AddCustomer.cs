using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProManSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodeClient = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    NomComplet = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Activite = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    Adresse = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    NumeroRC = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    MatriculeFiscal = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TypeIdentification = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    NumeroIdentification = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    CA_HT = table.Column<decimal>(type: "TEXT", nullable: true),
                    TauxTVA = table.Column<decimal>(type: "TEXT", nullable: true),
                    CA_TTC = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
