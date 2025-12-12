using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Linq;
using System.Windows.Controls;

namespace ProManSystem.Views
{
    public partial class DashboardView : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();

        public DashboardView()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var now = DateTime.Today;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

           
            var salesMonth = _db.SalesInvoices
                .Where(f => f.DateFacture >= firstDayOfMonth && f.DateFacture <= now)
                .ToList();

            decimal salesTtc = salesMonth.Sum(f => f.MontantTTC);
            int salesCount = salesMonth.Count;

            
            var purchasesMonth = _db.PurchaseInvoices
                .Where(f => f.DateFacture >= firstDayOfMonth && f.DateFacture <= now)
                .ToList();

            decimal purchasesTtc = purchasesMonth.Sum(f => f.MontantTTC);

            var lowStockProducts = _db.Products
                .Where(p => p.StockActuel <= p.StockMin)
                .OrderBy(p => p.CodeProduit)
                .ToList();

         
            var lastSales = _db.SalesInvoices
                .OrderByDescending(f => f.DateFacture)
                .Take(10)
                .ToList();

            
            SalesMonthTtcText.Text = salesTtc.ToString("0.00");
            PurchasesMonthTtcText.Text = purchasesTtc.ToString("0.00");
            SalesInvoicesCountText.Text = salesCount.ToString();
            LowStockProductsCountText.Text = lowStockProducts.Count.ToString();

            LastSalesGrid.ItemsSource = lastSales;
            LowStockGrid.ItemsSource = lowStockProducts;
        }
    }
}
