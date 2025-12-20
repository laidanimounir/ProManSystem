using Microsoft.EntityFrameworkCore;
using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProManSystem.Views
{
    public partial class SalesInvoicesView : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();

        private Customer? _selectedCustomer = null;
        private ObservableCollection<SalesInvoiceLine> _invoiceLines = new();
        private ObservableCollection<ProductWithMaxQuantity> _availableProducts = new();
        private ObservableCollection<RecipeDetail> _selectedProductRecipe = new();

        public SalesInvoicesView()
        {
            InitializeComponent();

            this.Loaded += SalesInvoicesView_Loaded;
        }

        private void SalesInvoicesView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitTvaList();
                PrepareNewInvoice();
                LoadAvailableProducts();

                InvoiceLinesGrid.ItemsSource = _invoiceLines;
                AvailableProductsGrid.ItemsSource = _availableProducts;
                ProductRecipeGrid.ItemsSource = _selectedProductRecipe;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في التحميل: {ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============================================
        // تهيئة قائمة TVA
        // ============================================
        private void InitTvaList()
        {
            var defaultRates = new[] { 19m, 9m, 0m };

            foreach (var rate in defaultRates)
                TvaComboBox.Items.Add(rate.ToString("0.##"));

            TvaComboBox.Text = "19";
        }

        private decimal GetTvaRate()
        {
            var txt = (TvaComboBox.Text ?? "0").Replace('.', ',');
            return decimal.TryParse(txt, out var t) ? t : 0m;
        }

        // ============================================
        // تحضير فاتورة جديدة
        // ============================================
        private void PrepareNewInvoice()
        {
            if (NumeroFactureTextBox != null)
                NumeroFactureTextBox.Text = GenerateInvoiceNumber();

            if (DateFacturePicker != null)
                DateFacturePicker.SelectedDate = DateTime.Today;

            _selectedCustomer = null;

            if (CustomerTextBox != null)
                CustomerTextBox.Text = string.Empty;

            _invoiceLines.Clear();

            if (MontantHTTextBox != null) MontantHTTextBox.Text = "0.00";
            if (MontantTVATextBox != null) MontantTVATextBox.Text = "0.00";
            if (MontantTTCTextBox != null) MontantTTCTextBox.Text = "0.00";
        }

        private string GenerateInvoiceNumber()
        {
            try
            {
                var last = _db.SalesInvoices
                    .OrderByDescending(f => f.Id)
                    .Select(f => f.NumeroFacture)
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(last))
                    return "FV001";

                if (!last.StartsWith("FV") || last.Length < 3)
                    return "FV001";

                if (!int.TryParse(last.Substring(2), out int num))
                    return "FV001";

                return "FV" + (num + 1).ToString("D3");
            }
            catch
            {
                return "FV001";
            }
        }

        // ============================================
        // تحميل المنتجات المتاحة مع حساب أقصى كمية
        // ============================================
        private void LoadAvailableProducts()
        {
            try
            {
                var products = _db.Products
     .Include(p => p.ProductRecipes)
         .ThenInclude(pr => pr.RawMaterial)
     .OrderBy(p => p.CodeProduit)
     .ToList();

                _availableProducts.Clear();

                foreach (var product in products)
                {
                    decimal maxQty = CalculateMaxQuantityFromRawMaterials(product);

                    _availableProducts.Add(new ProductWithMaxQuantity
                    {
                        Id = product.Id,
                        CodeProduit = product.CodeProduit,
                        Nom = product.Nom,
                        PrixVente = product.PrixVente,
                        MaxQuantity = maxQty,
                        Product = product
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل المنتجات: {ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private decimal CalculateMaxQuantityFromRawMaterials(Product product)
        {
            if (product.ProductRecipes == null)
                return 0m;

            // فقط الوصفات الصالحة
            var validRecipes = product.ProductRecipes
                .Where(r => r.RawMaterial != null && r.QuantiteNecessaire > 0 && r.RawMaterial.StockActuel > 0)
                .ToList();

            if (!validRecipes.Any())
                return 0m;

            // لكل مادة: المخزون / الكمية لكل وحدة
            var possibleQuantities = validRecipes
                .Select(r => r.RawMaterial.StockActuel / r.QuantiteNecessaire);

            // أقل قيمة هي الحد الأقصى للوحدات
            var maxQty = possibleQuantities.Min();

            return Math.Floor(maxQty);
        }


        // ============================================
        // عرض تفاصيل وصفة المنتج المختار
        // ============================================
        private void AvailableProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AvailableProductsGrid.SelectedItem is not ProductWithMaxQuantity selectedProduct)
            {
                _selectedProductRecipe.Clear();
                if (RecipeHeaderTextBlock != null)
                    RecipeHeaderTextBlock.Text = "📋 وصفة المنتج (اختر منتج من القائمة)";
                return;
            }

            LoadProductRecipe(selectedProduct.Product);
        }

        private void LoadProductRecipe(Product product)
        {
            try
            {
                _selectedProductRecipe.Clear();

                if (RecipeHeaderTextBlock != null)
                    RecipeHeaderTextBlock.Text = $"📋 وصفة المنتج: {product.Nom}";

                var recipes = _db.ProductRecipes
                    .Include(pr => pr.RawMaterial)
                    .Where(pr => pr.ProductId == product.Id)
                    .ToList();

                if (!recipes.Any())
                {
                    MessageBox.Show("هذا المنتج ليس له وصفة محددة!", "تنبيه",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                foreach (var recipe in recipes)
                {
                    if (recipe.RawMaterial == null)
                        continue;

                    decimal maxUnits = recipe.QuantiteNecessaire > 0
                        ? Math.Floor(recipe.RawMaterial.StockActuel / recipe.QuantiteNecessaire)
                        : 0m;

                    _selectedProductRecipe.Add(new RecipeDetail
                    {
                        RawMaterialName = recipe.RawMaterial.Designation,
                        QuantityPerUnit = recipe.QuantiteNecessaire,
                        AvailableStock = recipe.RawMaterial.StockActuel,
                        MaxUnits = maxUnits
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الوصفة: {ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============================================
        // إضافة منتج للفاتورة (نقر مزدوج)
        // ============================================
        private void AvailableProductsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AvailableProductsGrid.SelectedItem is not ProductWithMaxQuantity selectedProduct)
                return;

            if (selectedProduct.MaxQuantity <= 0)
            {
                MessageBox.Show("لا يمكن بيع هذا المنتج - المواد الأولية غير كافية!", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // طلب الكمية من المستخدم
            var dialog = new QuantityInputDialog(selectedProduct.MaxQuantity)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                decimal requestedQty = dialog.EnteredQuantity;

                if (requestedQty > selectedProduct.MaxQuantity)
                {
                    var result = MessageBox.Show(
                        $"الكمية المطلوبة ({requestedQty}) أكبر من المتاح ({selectedProduct.MaxQuantity}).\n" +
                        $"هل تريد المتابعة؟ (سيكون المخزون سالب)",
                        "تحذير",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                        return;
                }

                AddProductToInvoice(selectedProduct.Product, requestedQty);
            }
        }

        private void AddProductToInvoice(Product product, decimal quantity)
        {
            var line = new SalesInvoiceLine
            {
                ProductId = product.Id,
                Product = product,
                Quantite = quantity,
                PrixUnitaire = product.PrixVente,
                MontantLigne = quantity * product.PrixVente
            };

            _invoiceLines.Add(line);
            RecalculateTotals();
        }

        // ============================================
        // حذف سطر من الفاتورة
        // ============================================
        private void DeleteLineButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesInvoiceLine line)
            {
                _invoiceLines.Remove(line);
                RecalculateTotals();
            }
        }

        // ============================================
        // حساب الإجماليات
        // ============================================
        private void RecalculateTotals()
        {
            decimal ht = _invoiceLines.Sum(l => l.MontantLigne);
            decimal tvaRate = GetTvaRate() / 100m;
            decimal tva = Math.Round(ht * tvaRate, 2);
            decimal ttc = ht + tva;

            if (MontantHTTextBox != null)
                MontantHTTextBox.Text = ht.ToString("0.00");
            if (MontantTVATextBox != null)
                MontantTVATextBox.Text = tva.ToString("0.00");
            if (MontantTTCTextBox != null)
                MontantTTCTextBox.Text = ttc.ToString("0.00");
        }

        private void TvaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RecalculateTotals();
        }

        // ============================================
        // اختيار الزبون
        // ============================================
        private void PickCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new CustomerPickerWindow
            {
                Owner = Window.GetWindow(this)
            };

            if (win.ShowDialog() == true && win.SelectedCustomer != null)
            {
                _selectedCustomer = win.SelectedCustomer;
                if (CustomerTextBox != null)
                    CustomerTextBox.Text = $"{_selectedCustomer.CodeClient} - {_selectedCustomer.NomComplet}";
            }
        }

        // ============================================
        // حفظ الفاتورة
        // ============================================
        private void SaveInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            // التحقق من البيانات
            if (_selectedCustomer == null)
            {
                MessageBox.Show("اختر الزبون أولاً.", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_invoiceLines.Count == 0)
            {
                MessageBox.Show("أضف منتجات للفاتورة أولاً.", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DateFacturePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("حدد تاريخ الفاتورة.", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // قراءة القيم
            decimal ht = decimal.Parse(MontantHTTextBox.Text.Replace('.', ','));
            decimal tva = decimal.Parse(MontantTVATextBox.Text.Replace('.', ','));
            decimal ttc = decimal.Parse(MontantTTCTextBox.Text.Replace('.', ','));
            decimal tvaRate = GetTvaRate();

            try
            {
                // إنشاء الفاتورة
                var invoice = new SalesInvoice
                {
                    NumeroFacture = NumeroFactureTextBox.Text,
                    CustomerId = _selectedCustomer.Id,
                    DateFacture = DateFacturePicker.SelectedDate.Value,
                    MontantHT = ht,
                    TauxTVA = tvaRate,
                    MontantTVA = tva,
                    MontantTTC = ttc,
                    MontantPaye = ttc,
                    Reste = 0m,
                    EstPayee = true,
                    DateCreation = DateTime.Now
                };

                // إضافة السطور
                foreach (var line in _invoiceLines)
                {
                    invoice.Lignes.Add(new SalesInvoiceLine
                    {
                        ProductId = line.ProductId,
                        Quantite = line.Quantite,
                        PrixUnitaire = line.PrixUnitaire,
                        MontantLigne = line.MontantLigne
                    });
                }

                _db.SalesInvoices.Add(invoice);

                // المنطق الجديد: خصم من المواد الأولية وتحديث مخزون المنتج
                foreach (var line in invoice.Lignes)
                {
                    var product = _db.Products
                        .Include(p => p.ProductRecipes)
                        .ThenInclude(pr => pr.RawMaterial)
                        .First(p => p.Id == line.ProductId);

                    // خصم من المواد الأولية
                    foreach (var recipe in product.ProductRecipes)
                    {
                        var rawMaterial = recipe.RawMaterial;
                        decimal requiredQty = line.Quantite * recipe.QuantiteNecessaire;
                        rawMaterial.StockActuel -= requiredQty;
                    }

                    // تحديث مخزون المنتج (للعرض فقط)
                    decimal newProductStock = CalculateMaxQuantityFromRawMaterials(product);
                    product.StockActuel = newProductStock;
                }

                // تحديث CA الزبون
                var customer = _db.Customers.First(c => c.Id == invoice.CustomerId);
                customer.CA_TTC = (customer.CA_TTC ?? 0) + invoice.MontantTTC;

                _db.SaveChanges();

                MessageBox.Show("تم حفظ فاتورة البيع بنجاح.", "نجح",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // إعادة تحميل
                PrepareNewInvoice();
                LoadAvailableProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء الحفظ: {ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============================================
        // مسح النموذج
        // ============================================
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            PrepareNewInvoice();
            LoadAvailableProducts();
        }
    }

    // ============================================
    // كلاسات مساعدة
    // ============================================
    public class ProductWithMaxQuantity
    {
        public int Id { get; set; }
        public string CodeProduit { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public decimal PrixVente { get; set; }
        public decimal MaxQuantity { get; set; }
        public Product Product { get; set; } = null!;
    }

    public class RecipeDetail
    {
        public string RawMaterialName { get; set; } = string.Empty;
        public decimal QuantityPerUnit { get; set; }
        public decimal AvailableStock { get; set; }
        public decimal MaxUnits { get; set; }
    }
}
