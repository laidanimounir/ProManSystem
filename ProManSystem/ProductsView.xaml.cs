using Microsoft.EntityFrameworkCore;
using ProManSystem.Data;
using ProManSystem.Models;
using ProManSystem.Services;
using System;
using System.Collections.Generic;
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

            UpdateProductDetails(null);
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
                    UpdateProductDetails(null);
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

        private void ProduceProductButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is not Product product)
                return;

            var input = Microsoft.VisualBasic.Interaction.InputBox(
                "أدخل كمية الإنتاج المطلوبة:",
                "إنتاج المنتج",
                "1");

            if (string.IsNullOrWhiteSpace(input))
                return;

            if (!decimal.TryParse(input.Replace('.', ','), out decimal qty) || qty <= 0)
            {
                MessageBox.Show("كمية غير صالحة.", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var service = new ProductionService(_db);
                var results = service.CalculateConsumption(product.Id, qty);

                if (results == null || results.Count == 0)
                {
                    MessageBox.Show("لا توجد وصفة لهذا المنتج. الرجاء تعريف المواد الأولية أولاً.",
                        "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var negatives = results.Where(r => r.StockAfter < 0).ToList();

                if (negatives.Any())
                {
                    string msg = "بعض المواد لن يكفي مخزونها لهذا الإنتاج:\n\n";
                    foreach (var r in negatives)
                    {
                        msg += $"{r.RawMaterialName}: المخزون الحالي = {r.CurrentStock}, " +
                               $"المطلوب = {r.QuantityNeeded}, " +
                               $"بعد الإنتاج = {r.StockAfter}\n";
                    }
                    msg += "\nهل تريد المتابعة وجعل المخزون سالبًا لهذه المواد؟";

                    var res = MessageBox.Show(msg, "تحذير مخزون",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (res != MessageBoxResult.Yes)
                        return;
                }

                ApplyProduction(product.Id, qty, results);

                MessageBox.Show("تم تنفيذ الإنتاج وتحديث المخزون.", "تم",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                LoadProducts(SearchTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء عملية الإنتاج: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyProduction(int productId, decimal quantityToProduce,
                                     List<ConsumptionResult> results)
        {
            foreach (var r in results)
            {
                var rm = _db.RawMaterials.First(x => x.Id == r.RawMaterialId);
                rm.StockActuel = r.StockAfter;
            }

            var product = _db.Products.First(x => x.Id == productId);
            product.StockActuel += quantityToProduce;

            _db.SaveChanges();
        }

        private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var product = ProductsGrid.SelectedItem as Product;
            UpdateProductDetails(product);
        }

        private void UpdateProductDetails(Product? p)
        {
            if (p == null)
            {
                DetailCodeTextBlock.Text = string.Empty;
                DetailNameTextBlock.Text = string.Empty;
                DetailDateTextBlock.Text = string.Empty;
                DetailStockTextBlock.Text = string.Empty;
                DetailStockMinTextBlock.Text = string.Empty;
                DetailCoutTextBlock.Text = string.Empty;
                DetailPrixTextBlock.Text = string.Empty;
                DetailMargeTextBlock.Text = string.Empty;
                DetailDescriptionTextBlock.Text = string.Empty;
                return;
            }

            DetailCodeTextBlock.Text = p.CodeProduit;
            DetailNameTextBlock.Text = p.Nom;
            DetailDateTextBlock.Text = p.DateCreation.ToString("yyyy-MM-dd HH:mm");
            DetailStockTextBlock.Text = p.StockActuel.ToString("N2");
            DetailStockMinTextBlock.Text = p.StockMin.ToString("N2");
            DetailCoutTextBlock.Text = p.CoutProduction.ToString("N2");
            DetailPrixTextBlock.Text = p.PrixVente.ToString("N2");
            DetailMargeTextBlock.Text = p.Marge.ToString("N2");
            DetailDescriptionTextBlock.Text = string.IsNullOrWhiteSpace(p.Description)
                ? "-"
                : p.Description;
        }

      

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsGrid == null || ProductsGrid.ItemsSource == null)
                return;
        }

        private void ProductsGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            
        }

        private void ExportReportButton_Click(object sender, RoutedEventArgs e)
        {
            var product = ProductsGrid.SelectedItem as Product;

            if (product == null)
            {
                MessageBox.Show("الرجاء تحديد منتج أولاً.", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBox.Show(
                $"سيتم تصدير تقرير المنتج '{product.Nom}' إلى PDF\n\n(سيتم تطبيقه لاحقاً)",
                "تصدير PDF",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
