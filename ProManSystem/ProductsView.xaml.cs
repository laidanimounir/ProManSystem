using Microsoft.EntityFrameworkCore;
using ProManSystem.Data;
using ProManSystem.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProManSystem.Views
{
    public partial class ProductsView : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();
        private ObservableCollection<Product> _products = new();

        public ProductsView()
        {
            InitializeComponent();
            LoadProducts();
        }

        private void LoadProducts(string? searchTerm = null)
        {
            IQueryable<Product> query = _db.Products;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(p =>
                    p.CodeProduit.Contains(searchTerm) ||
                    p.Nom.Contains(searchTerm));
            }

            var list = query
                .OrderBy(p => p.CodeProduit)
                .ToList();

            _products = new ObservableCollection<Product>(list);
            ProductsGrid.ItemsSource = _products;
        }

        private Window? GetOwnerWindow()
        {
            return Window.GetWindow(this);
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new ProductEditorWindow
            {
                Owner = GetOwnerWindow()
            };

            if (win.ShowDialog() == true)
                LoadProducts(SearchTextBox.Text);
        }

        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Product p)
            {
                var win = new ProductEditorWindow(p.Id)
                {
                    Owner = GetOwnerWindow()
                };

                if (win.ShowDialog() == true)
                    LoadProducts(SearchTextBox.Text);
            }
        }

        private void EditRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is Product p)
            {
                var win = new ProductRecipeWindow(p.Id)
                {
                    Owner = GetOwnerWindow()
                };

                win.ShowDialog();
            }
        }

        private void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Product p)
            {
                var result = MessageBox.Show(
                    $"هل تريد حذف المنتج '{p.Nom}'؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _db.Products.Remove(p);
                    _db.SaveChanges();
                    _products.Remove(p);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts(SearchTextBox.Text);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string term = (SearchTextBox.Text ?? string.Empty).Trim();
            LoadProducts(string.IsNullOrWhiteSpace(term) ? null : term);
        }
    }
}
