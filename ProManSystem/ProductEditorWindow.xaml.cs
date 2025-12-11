using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace ProManSystem.Views
{
    public partial class ProductEditorWindow : Window
    {
        private readonly AppDbContext _db = new AppDbContext();
        private readonly int? _productId;

        public ProductEditorWindow(int? productId = null)
        {
            InitializeComponent();
            _productId = productId;

            if (_productId.HasValue)
            {
               
                LoadProduct();
            }
            else
            {
             
                CodeTextBox.Text = GenerateNextProductCode();
            }
        }

        private void LoadProduct()
        {
            var p = _db.Products.FirstOrDefault(x => x.Id == _productId.Value);
            if (p == null) return;

            CodeTextBox.Text = p.CodeProduit;
            NameTextBox.Text = p.Nom;
            DescriptionTextBox.Text = p.Description;
            StockMinTextBox.Text = p.StockMin.ToString("0.##");
            PrixVenteTextBox.Text = p.PrixVente.ToString("0.##");
        }

       
        private string GenerateNextProductCode()
        {
            const string prefix = "PR";
            const int numberLength = 4; 

           
            var existingCodes = _db.Products
                .Where(p => p.CodeProduit != null && p.CodeProduit.StartsWith(prefix))
                .Select(p => p.CodeProduit)
                .ToList();

            if (existingCodes.Count == 0)
            {
               
                return $"{prefix}{1.ToString(new string('0', numberLength))}";
            }

            int maxNumber = 0;
            var regex = new Regex($@"^{prefix}(\d+)$");

            foreach (var code in existingCodes)
            {
                var match = regex.Match(code);
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value, out int n))
                    {
                        if (n > maxNumber)
                            maxNumber = n;
                    }
                }
            }

            int nextNumber = maxNumber + 1;
            string formattedNumber = nextNumber.ToString(new string('0', numberLength));
            return $"{prefix}{formattedNumber}";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CodeTextBox.Text) ||
                string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("الرجاء إدخال كود واسم المنتج.", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(StockMinTextBox.Text.Replace('.', ','), out decimal stockMin))
                stockMin = 0;

            if (!decimal.TryParse(PrixVenteTextBox.Text.Replace('.', ','), out decimal prixVente))
                prixVente = 0;

            Product product;

            if (_productId.HasValue)
            {
                product = _db.Products.First(x => x.Id == _productId.Value);
            }
            else
            {
                product = new Product
                {
                    DateCreation = DateTime.Now
                };
                _db.Products.Add(product);

            
                if (string.IsNullOrWhiteSpace(CodeTextBox.Text))
                    CodeTextBox.Text = GenerateNextProductCode();
            }

            product.CodeProduit = CodeTextBox.Text.Trim();
            product.Nom = NameTextBox.Text.Trim();
            product.Description = string.IsNullOrWhiteSpace(DescriptionTextBox.Text)
                ? null
                : DescriptionTextBox.Text.Trim();
            product.StockMin = stockMin;
            product.PrixVente = prixVente;

            product.CoutProduction = 0;
            product.Marge = prixVente;

            _db.SaveChanges();
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
