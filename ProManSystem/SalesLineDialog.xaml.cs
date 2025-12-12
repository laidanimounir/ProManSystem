using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProManSystem.Views
{
    public partial class SalesLineDialog : Window
    {
        private readonly AppDbContext _db = new AppDbContext();

        public SalesInvoiceLine? CreatedLine { get; private set; }

        public SalesLineDialog()
        {
            InitializeComponent();
            LoadProducts();

            QuantiteTextBox.TextChanged += AnyField_TextChanged;
            PrixTextBox.TextChanged += AnyField_TextChanged;
        }

        private void LoadProducts()
        {
            try
            {
                var products = _db.Products
                    .OrderBy(p => p.CodeProduit)
                    .ToList();

                ProductComboBox.ItemsSource = products;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des produits : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AnyField_TextChanged(object sender, TextChangedEventArgs e)
        {
            var qTxt = (QuantiteTextBox.Text ?? "0").Replace('.', ',');
            var pTxt = (PrixTextBox.Text ?? "0").Replace('.', ',');

            if (!decimal.TryParse(qTxt, out var q))
                q = 0;
            if (!decimal.TryParse(pTxt, out var p))
                p = 0;

            var total = q * p;
            TotalTextBox.Text = total.ToString("0.00");
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductComboBox.SelectedItem is not Product product)
            {
                MessageBox.Show("اختر منتجًا أولاً.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var qTxt = (QuantiteTextBox.Text ?? "").Replace('.', ',');
            var pTxt = (PrixTextBox.Text ?? "").Replace('.', ',');

            if (!decimal.TryParse(qTxt, out var q) || q <= 0)
            {
                MessageBox.Show("الكمية غير صحيحة.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(pTxt, out var p) || p < 0)
            {
                MessageBox.Show("السعر غير صحيح.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

           
            if (q > product.StockActuel)
            {
                MessageBox.Show($"المخزون غير كافٍ! المتوفر: {product.StockActuel}",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CreatedLine = new SalesInvoiceLine
            {
                ProductId = product.Id,
                Product = product,
                Quantite = q,
                PrixUnitaire = p,
                MontantLigne = q * p
            };

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
