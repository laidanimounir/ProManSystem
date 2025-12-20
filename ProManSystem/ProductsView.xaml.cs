using Microsoft.EntityFrameworkCore;
using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace ProManSystem.Views
{
    public partial class ProductsView : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();
        private ObservableCollection<Product> _products = new();
        private ObservableCollection<ProductRecipe> _recipeRows = new();
        private List<RawMaterial> _allRawMaterials = new();

        private int? _currentProductId = null;
        private bool _isEditMode = false;
        private bool _isCalculating = false; 

        public ProductsView()
        {
            InitializeComponent();

            
            this.Loaded += ProductsView_Loaded;
        }

        private void ProductsView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadProducts();
                LoadAvailableRawMaterials();

                _recipeRows = new ObservableCollection<ProductRecipe>();
                RecipeGrid.ItemsSource = _recipeRows;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void LoadAvailableRawMaterials()
        {
            _allRawMaterials = _db.RawMaterials
                .Include(r => r.Unit)
                .OrderBy(r => r.Designation)
                .ToList();

            AvailableRawMaterialsGrid.ItemsSource = _allRawMaterials;

           
            if (RecipeGrid != null && RecipeGrid.Columns.Count > 0)
            {
                var comboColumn = RecipeGrid.Columns[0] as DataGridComboBoxColumn;
                if (comboColumn != null)
                {
                    comboColumn.ItemsSource = _allRawMaterials;
                }
            }
        }


     
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string term = (SearchTextBox.Text ?? string.Empty).Trim();
            LoadProducts(string.IsNullOrWhiteSpace(term) ? null : term);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts(SearchTextBox.Text);
            LoadAvailableRawMaterials();
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            StartNewProduct();
        }

        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Product p)
            {
                LoadProductForEdit(p.Id);
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
                    ClearEditorPanel();
                }
            }
        }

        private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // يمكن عرض المنتج المختار تلقائيًا أو تركه للمستخدم
            // حاليًا نتركه فارغًا حتى يضغط على "تعديل"
        }

       
        private void StartNewProduct()
        {
            _currentProductId = null;
            _isEditMode = false;

            if (EditorTitleTextBlock != null)
                EditorTitleTextBlock.Text = "➕ إضافة منتج جديد";

            if (CodeTextBox != null)
                CodeTextBox.Text = GenerateNextProductCode();
            if (NameTextBox != null)
                NameTextBox.Text = string.Empty;
            if (DescriptionTextBox != null)
                DescriptionTextBox.Text = string.Empty;

            _recipeRows.Clear();
            if (RecipeGrid != null)
                RecipeGrid.ItemsSource = _recipeRows;

            if (ProductionCostTextBlock != null)
                ProductionCostTextBlock.Text = "0.00";
            if (MaxProductionTextBlock != null)
                MaxProductionTextBlock.Text = "--";
            if (MarginPercentageTextBox != null)
                MarginPercentageTextBox.Text = "30";
            if (MarginFixedTextBox != null)
                MarginFixedTextBox.Text = "0";
            if (MarginPercentageRadio != null)
                MarginPercentageRadio.IsChecked = true;
            if (FinalPriceTextBox != null)
                FinalPriceTextBox.Text = "0";
        }

        
        private void LoadProductForEdit(int productId)
        {
            var product = _db.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) return;

            _currentProductId = productId;
            _isEditMode = true;

            if (EditorTitleTextBlock != null)
                EditorTitleTextBlock.Text = "✏️ تعديل المنتج";

            if (CodeTextBox != null)
                CodeTextBox.Text = product.CodeProduit;
            if (NameTextBox != null)
                NameTextBox.Text = product.Nom;
            if (DescriptionTextBox != null)
                DescriptionTextBox.Text = product.Description ?? string.Empty;

           
            var recipes = _db.ProductRecipes
                .Where(pr => pr.ProductId == productId)
                .OrderBy(pr => pr.Id)
                .ToList();

            _recipeRows = new ObservableCollection<ProductRecipe>(recipes);
            if (RecipeGrid != null)
                RecipeGrid.ItemsSource = _recipeRows;

          
            RecalculatePricing();
        }

      
        private void ClearEditorPanel()
        {
            _currentProductId = null;
            _isEditMode = false;

            if (EditorTitleTextBlock != null)
                EditorTitleTextBlock.Text = "✏️ تحرير المنتج";

            if (CodeTextBox != null)
                CodeTextBox.Text = string.Empty;
            if (NameTextBox != null)
                NameTextBox.Text = string.Empty;
            if (DescriptionTextBox != null)
                DescriptionTextBox.Text = string.Empty;

            if (_recipeRows != null)
                _recipeRows.Clear();
            if (RecipeGrid != null)
                RecipeGrid.ItemsSource = _recipeRows;

            if (ProductionCostTextBlock != null)
                ProductionCostTextBlock.Text = "0.00";
            if (MaxProductionTextBlock != null)
                MaxProductionTextBlock.Text = "--";
            if (FinalPriceTextBox != null)
                FinalPriceTextBox.Text = "0";
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

       
        private void AddRecipeRowButton_Click(object sender, RoutedEventArgs e)
        {
            var newRow = new ProductRecipe
            {
                ProductId = _currentProductId ?? 0,
                QuantiteNecessaire = 0
            };
            _recipeRows.Add(newRow);
            RefreshRecipeGrid();
        }

        private void DeleteRecipeRowButton_Click(object sender, RoutedEventArgs e)
        {
            var row = (sender as FrameworkElement)?.DataContext as ProductRecipe;
            if (row == null) return;

            _recipeRows.Remove(row);
            RefreshRecipeGrid();
           // RecalculatePricing();
        }

        private void RefreshRecipeGrid()
        {
            RecipeGrid.ItemsSource = null;
            RecipeGrid.ItemsSource = _recipeRows;
        }

  
        private void RecalculateButton_Click(object sender, RoutedEventArgs e)
        {
            RecalculatePricing();
        }

        private void RecalculatePricing()
        {
            if (_isCalculating) return;
            _isCalculating = true;

            try
            {
               
                if (ProductionCostTextBlock == null || MaxProductionTextBlock == null)
                {
                    _isCalculating = false;
                    return;
                }

                // 1. حساب تكلفة الإنتاج
                decimal totalCost = 0m;
                decimal maxProduction = decimal.MaxValue;

                foreach (var recipe in _recipeRows)
                {
                    if (recipe.RawMaterialId == 0 || recipe.QuantiteNecessaire <= 0)
                        continue;

                    var rawMaterial = _allRawMaterials.FirstOrDefault(r => r.Id == recipe.RawMaterialId);
                    if (rawMaterial == null) continue;

                    totalCost += recipe.QuantiteNecessaire * rawMaterial.PMAPA;

                    if (recipe.QuantiteNecessaire > 0)
                    {
                        decimal possibleQty = rawMaterial.StockActuel / recipe.QuantiteNecessaire;
                        if (possibleQty < maxProduction)
                            maxProduction = possibleQty;
                    }
                }

                ProductionCostTextBlock.Text = totalCost.ToString("N2");

                if (maxProduction == decimal.MaxValue || maxProduction < 0)
                    MaxProductionTextBlock.Text = "--";
                else
                    MaxProductionTextBlock.Text = Math.Floor(maxProduction).ToString("N0");

                
                UpdateFinalPriceFromMargin(totalCost);
            }
            finally
            {
                _isCalculating = false;
            }
        }
        //  حساب سعر البيع
        private void UpdateFinalPriceFromMargin(decimal cost)
        {
            if (_isCalculating) return;

            // فحص جاهزية العناصر - هذا هو الحل الجذري
            if (MarginPercentageRadio == null ||
                MarginFixedRadio == null ||
                MarginPercentageTextBox == null ||
                MarginFixedTextBox == null ||
                FinalPriceTextBox == null)
            {
                return; 
            }

            decimal margin = 0m;

            if (MarginPercentageRadio.IsChecked == true)
            {
                if (decimal.TryParse(MarginPercentageTextBox.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal percentage))
                {
                    margin = cost * (percentage / 100m);
                }
            }
            else if (MarginFixedRadio.IsChecked == true)
            {
                if (decimal.TryParse(MarginFixedTextBox.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal fixedMargin))
                {
                    margin = fixedMargin;
                }
            }

            decimal finalPrice = cost + margin;
            FinalPriceTextBox.Text = finalPrice.ToString("N2");
        }

        
        private void MarginType_Changed(object sender, RoutedEventArgs e)
        {
            if (_isCalculating) return;

            if (decimal.TryParse(ProductionCostTextBlock.Text.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal cost))
            {
                UpdateFinalPriceFromMargin(cost);
            }
        }

        private void PricingField_Changed(object sender, TextChangedEventArgs e)
        {
            if (_isCalculating) return;

            if (decimal.TryParse(ProductionCostTextBlock.Text.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal cost))
            {
                UpdateFinalPriceFromMargin(cost);
            }
        }

        private void FinalPrice_Changed(object sender, TextChangedEventArgs e)
        {
            // يمكن إضافة منطق لإعادة حساب الهامش إذا تم تعديل السعر يدويًا
            // الآن يبقى بسيط
        }

        
        private void SaveProductButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(CodeTextBox.Text) ||
                string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("الرجاء إدخال كود واسم المنتج.", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
            if (_recipeRows.Any(r => r.RawMaterialId == 0))
            {
                MessageBox.Show("الرجاء اختيار مادة أولية لكل سطر في الوصفة.", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

           
            if (!decimal.TryParse(FinalPriceTextBox.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal finalPrice))
            {
                finalPrice = 0;
            }

           
            if (!decimal.TryParse(ProductionCostTextBlock.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal cost))
            {
                cost = 0;
            }

            try
            {
                Product product;

                if (_isEditMode && _currentProductId.HasValue)
                {
                  
                    product = _db.Products.First(x => x.Id == _currentProductId.Value);
                }
                else
                {
                 
                    product = new Product
                    {
                        DateCreation = DateTime.Now
                    };
                    _db.Products.Add(product);
                }

                product.CodeProduit = CodeTextBox.Text.Trim();
                product.Nom = NameTextBox.Text.Trim();
                product.Description = string.IsNullOrWhiteSpace(DescriptionTextBox.Text)
                    ? null
                    : DescriptionTextBox.Text.Trim();
                product.PrixVente = finalPrice;
                product.CoutProduction = cost;
                product.Marge = finalPrice - cost;

                // حفظ المنتج أولاً للحصول على Id
                _db.SaveChanges();

                // حفظ الوصفة
                var existing = _db.ProductRecipes
                    .Where(pr => pr.ProductId == product.Id)
                    .ToList();
                _db.ProductRecipes.RemoveRange(existing);

                foreach (var row in _recipeRows.Where(r => r.RawMaterialId != 0 && r.QuantiteNecessaire > 0))
                {
                    row.Id = 0;
                    row.ProductId = product.Id;
                    _db.ProductRecipes.Add(row);
                }

                _db.SaveChanges();

                MessageBox.Show("تم حفظ المنتج بنجاح.", "تم",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                LoadProducts(SearchTextBox.Text);
                ClearEditorPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الحفظ: {ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

      
        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            ClearEditorPanel();
        }
    }
}
