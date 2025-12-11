using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ProManSystem.Models;

namespace ProManSystem.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<RawMaterial> RawMaterials { get; set; }
        public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }
        public DbSet<PurchaseInvoiceLine> PurchaseInvoiceLines { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var dbPath = Path.Combine(folder, "app.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}
