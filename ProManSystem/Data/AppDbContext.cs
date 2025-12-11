using Microsoft.EntityFrameworkCore;
using ProManSystem.Models;
using System.IO;
using System.Reflection;

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

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductRecipe> ProductRecipes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var dbPath = Path.Combine(folder, "app.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

          
            modelBuilder.Entity<ProductRecipe>()
                .HasOne(pr => pr.Product)
                .WithMany(p => p.ProductRecipes)   
                .HasForeignKey(pr => pr.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

         
            modelBuilder.Entity<ProductRecipe>()
                .HasOne(pr => pr.RawMaterial)
                .WithMany(r => r.ProductRecipes)  
                .HasForeignKey(pr => pr.RawMaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
