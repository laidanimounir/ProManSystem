using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ProManSystem.Models;

namespace ProManSystem.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var dbPath = Path.Combine(folder, "app.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}
