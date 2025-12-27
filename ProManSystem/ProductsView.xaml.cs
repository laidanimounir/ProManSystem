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
using System.Windows.Media;
using System.Windows.Threading;

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
        private DispatcherTimer _searchTimer = null!; 

        public ProductsView()
        {
            InitializeComponent();
            this.Loaded += ProductsView_Loaded;

            
            _searchTimer = new DispatcherTimer();
            _searchTimer.Interval = TimeSpan.FromMilliseconds(300);
            _searchTimer.Tick += SearchTimer_Tick;
        }

        private void ProductsView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadProducts();
                LoadAvailableRawMaterials();
                _recipeRows = new ObservableCollection<ProductRecipe>();
                RecipeGrid.ItemsSource = _recipeRows;
                UpdateStatistics();
                UpdateStatus("Prêt", Brushes.LimeGreen);
            }
            catch (Exception ex)
            {
                ShowError($"Erreur de chargement: {ex.Message}");
            }
        }

        #region Data Loading

        private void LoadProducts(string? searchTerm = null)
        {
            try
            {
                IQueryable<Product> query = _db.Products;

                if (!string.IsNullOrWhiteSpace(searchTerm) && searchTerm != "Rechercher par code ou nom...")
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

                UpdateStatistics();
                UpdateStatus($"{_products.Count} produit(s) chargé(s)", Brushes.LimeGreen);
            }
            catch (Exception ex)
            {
                ShowError($"Erreur de chargement des produits: {ex.Message}");
            }
        }

        private void LoadAvailableRawMaterials()
        {
            try
            {
                _allRawMaterials = _db.RawMaterials
                    .Include(r => r.Unit)
                    .OrderBy(r => r.Designation)
                    .ToList();

                PopupMaterialsGrid.ItemsSource = _allRawMaterials;

                if (RecipeGrid != null && RecipeGrid.Columns.Count > 0)
                {
                    var comboColumn = RecipeGrid.Columns[0] as DataGridComboBoxColumn;
                    if (comboColumn != null)
                    {
                        comboColumn.ItemsSource = _allRawMaterials;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erreur de chargement des matières: {ex.Message}");
            }
        }

        #endregion

        #region Search & Filter

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            string term = SearchTextBox.Text?.Trim() ?? "";
            LoadProducts(string.IsNullOrWhiteSpace(term) || term == "Rechercher par code ou nom..." ? null : term);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string term = SearchTextBox.Text?.Trim() ?? "";
            LoadProducts(string.IsNullOrWhiteSpace(term) || term == "Rechercher par code ou nom..." ? null : term);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
            LoadAvailableRawMaterials();
            ShowSuccess("Données actualisées");
        }

        #endregion

        #region Product CRUD Operations

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
                    $"Êtes-vous sûr de vouloir supprimer le produit '{p.Nom}'?\n\nCette action est irréversible.",
                    "Confirmer la suppression",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _db.Products.Remove(p);
                        _db.SaveChanges();
                        _products.Remove(p);
                        ClearEditorPanel();
                        UpdateStatistics();
                        ShowSuccess("Produit supprimé avec succès");
                    }
                    catch (Exception ex)
                    {
                        ShowError($"Erreur de suppression: {ex.Message}");
                    }
                }
            }
        }

        private void SaveProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateProduct())
                return;

            try
            {
                UpdateStatus("Enregistrement en cours...", Brushes.Orange);
                SaveProductButton.IsEnabled = false;

                string newCode = CodeTextBox.Text.Trim();
                if (IsProductCodeDuplicate(newCode, _currentProductId))
                {
                    ShowError("Ce code produit existe déjà. Veuillez utiliser un autre code.");
                    SaveProductButton.IsEnabled = true;
                    return;
                }

                if (!decimal.TryParse(FinalPriceTextBox.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal finalPrice))
                {
                    finalPrice = 0;
                }

                if (!decimal.TryParse(ProductionCostTextBlock.Text.Replace(" DA", "").Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal cost))
                {
                    cost = 0;
                }

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
                product.PrixVente = finalPrice;
                product.CoutProduction = cost;
                product.Marge = finalPrice - cost;

                _db.SaveChanges();

                // update recipes
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

                LoadProducts();
                ClearEditorPanel();
                UpdateStatistics();
                ShowSuccess("Produit enregistré avec succès");
                SaveProductButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors de l'enregistrement: {ex.Message}");
                SaveProductButton.IsEnabled = true;
            }
        }

        #endregion

        #region Product Editor

        private void StartNewProduct()
        {
            _currentProductId = null;
            _isEditMode = false;

            if (EditorTitleTextBlock != null)
                EditorTitleTextBlock.Text = "➕ Nouveau Produit";

            if (CodeTextBox != null)
                CodeTextBox.Text = GenerateNextProductCode();

            if (NameTextBox != null)
                NameTextBox.Text = string.Empty;

            _recipeRows.Clear();

            if (RecipeGrid != null)
                RecipeGrid.ItemsSource = _recipeRows;

            if (ProductionCostTextBlock != null)
                ProductionCostTextBlock.Text = "0.00 DA";

            if (MaxProductionTextBlock != null)
                MaxProductionTextBlock.Text = "-- unités";

            if (MarginPercentageTextBox != null)
                MarginPercentageTextBox.Text = "30";

            if (MarginFixedTextBox != null)
                MarginFixedTextBox.Text = "0";

            if (MarginPercentageRadio != null)
                MarginPercentageRadio.IsChecked = true;

            if (FinalPriceTextBox != null)
                FinalPriceTextBox.Text = "0";

            UpdateStatus("Mode création", Brushes.DeepSkyBlue);
        }

        private void LoadProductForEdit(int productId)
        {
            try
            {
                var product = _db.Products.FirstOrDefault(p => p.Id == productId);
                if (product == null) return;

                _currentProductId = productId;
                _isEditMode = true;

                if (EditorTitleTextBlock != null)
                    EditorTitleTextBlock.Text = "✏️ Modifier le Produit";

                if (CodeTextBox != null)
                    CodeTextBox.Text = product.CodeProduit;

                if (NameTextBox != null)
                    NameTextBox.Text = product.Nom;

                var recipes = _db.ProductRecipes
                    .Where(pr => pr.ProductId == productId)
                    .OrderBy(pr => pr.Id)
                    .ToList();

                _recipeRows = new ObservableCollection<ProductRecipe>(recipes);

                if (RecipeGrid != null)
                    RecipeGrid.ItemsSource = _recipeRows;

                RecalculatePricing();
                UpdateStatus("Mode édition", Brushes.Orange);
            }
            catch (Exception ex)
            {
                ShowError($"Erreur de chargement du produit: {ex.Message}");
            }
        }

        private void ClearEditorPanel()
        {
            _currentProductId = null;
            _isEditMode = false;

            if (EditorTitleTextBlock != null)
                EditorTitleTextBlock.Text = "✏️ Éditer le Produit";

            if (CodeTextBox != null)
                CodeTextBox.Text = string.Empty;

            if (NameTextBox != null)
                NameTextBox.Text = string.Empty;

            if (_recipeRows != null)
                _recipeRows.Clear();

            if (RecipeGrid != null)
                RecipeGrid.ItemsSource = _recipeRows;

            if (ProductionCostTextBlock != null)
                ProductionCostTextBlock.Text = "0.00 DA";

            if (MaxProductionTextBlock != null)
                MaxProductionTextBlock.Text = "-- unités";

            if (FinalPriceTextBox != null)
                FinalPriceTextBox.Text = "0";

            UpdateStatus("Prêt", Brushes.LimeGreen);
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Voulez-vous annuler les modifications?\n\nToutes les modifications non enregistrées seront perdues.",
                "Confirmer l'annulation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ClearEditorPanel();
            }
        }

        private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //  Auto-load on selection
        }

        #endregion

        #region Recipe Management

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
            RecalculatePricing();
        }

        private void RefreshRecipeGrid()
        {
            RecipeGrid.ItemsSource = null;
            RecipeGrid.ItemsSource = _recipeRows;
        }

        #endregion

        #region Material Selection Popup

        private void ChooseMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            MaterialPopup.Visibility = Visibility.Visible;
            PopupSearchTextBox.Text = string.Empty;
            PopupMaterialsGrid.ItemsSource = _allRawMaterials;
        }

        private void ClosePopupButton_Click(object sender, RoutedEventArgs e)
        {
            MaterialPopup.Visibility = Visibility.Collapsed;
        }

        private void SelectMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = PopupMaterialsGrid.SelectedItem as RawMaterial;
            if (selected == null)
            {
                MessageBox.Show("Veuillez sélectionner une matière première.", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // add to recipe
            var newRow = new ProductRecipe
            {
                ProductId = _currentProductId ?? 0,
                RawMaterialId = selected.Id,
                QuantiteNecessaire = 1
            };
            _recipeRows.Add(newRow);
            RefreshRecipeGrid();
            RecalculatePricing();

            MaterialPopup.Visibility = Visibility.Collapsed;
            ShowSuccess($"Matière '{selected.Designation}' ajoutée");
        }

        private void PopupSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchTerm = PopupSearchTextBox.Text?.Trim().ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                PopupMaterialsGrid.ItemsSource = _allRawMaterials;
            }
            else
            {
                var filtered = _allRawMaterials.Where(r =>
                    (r.CodeMatiere?.ToLower().Contains(searchTerm) ?? false) ||
                    (r.Designation?.ToLower().Contains(searchTerm) ?? false)
                ).ToList();

                PopupMaterialsGrid.ItemsSource = filtered;
            }
        }

        #endregion

        #region Pricing Calculations

        private void RecalculateButton_Click(object sender, RoutedEventArgs e)
        {
            RecalculatePricing();
            ShowSuccess("Calculs mis à jour");
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

                ProductionCostTextBlock.Text = totalCost.ToString("N2") + " DA";

                if (maxProduction == decimal.MaxValue || maxProduction < 0)
                    MaxProductionTextBlock.Text = "-- unités";
                else
                    MaxProductionTextBlock.Text = Math.Floor(maxProduction).ToString("N0") + " unités";

                UpdateFinalPriceFromMargin(totalCost);
            }
            finally
            {
                _isCalculating = false;
            }
        }

        private void UpdateFinalPriceFromMargin(decimal cost)
        {
            if (_isCalculating) return;

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

            if (ProductionCostTextBlock != null)
            {
                string costText = ProductionCostTextBlock.Text.Replace(" DA", "").Replace(',', '.');
                if (decimal.TryParse(costText,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal cost))
                {
                    UpdateFinalPriceFromMargin(cost);
                }
            }
        }

        private void MarginPercentageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isCalculating) return;
            if (MarginPercentageRadio?.IsChecked != true) return;

            if (ProductionCostTextBlock != null)
            {
                string costText = ProductionCostTextBlock.Text.Replace(" DA", "").Replace(',', '.');
                if (decimal.TryParse(costText,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal cost))
                {
                    UpdateFinalPriceFromMargin(cost);
                }
            }
        }

        private void MarginFixedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isCalculating) return;
            if (MarginFixedRadio?.IsChecked != true) return;

            if (ProductionCostTextBlock != null)
            {
                string costText = ProductionCostTextBlock.Text.Replace(" DA", "").Replace(',', '.');
                if (decimal.TryParse(costText,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal cost))
                {
                    UpdateFinalPriceFromMargin(cost);
                }
            }
        }

        private void FinalPrice_Changed(object sender, TextChangedEventArgs e)
        {
            //Reverse calculate margin if user manually edits final price
        }

        #endregion

        #region Input Validation

        private void QuantityTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            var regex = new Regex(@"^[0-9,\.]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void QuantityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string text = textBox.Text.Replace(',', '.');

            if (decimal.TryParse(text, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal value))
            {
                textBox.Text = value.ToString("N3");

                if (textBox.DataContext is ProductRecipe recipe)
                {
                    recipe.QuantiteNecessaire = value;
                    RecalculatePricing();
                }
            }
        }

        private bool ValidateProduct()
        {
            if (string.IsNullOrWhiteSpace(CodeTextBox.Text))
            {
                ShowError("Le code produit est obligatoire.");
                CodeTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ShowError("Le nom du produit est obligatoire.");
                NameTextBox.Focus();
                return false;
            }

            if (_recipeRows.Any(r => r.RawMaterialId == 0))
            {
                ShowError("Veuillez sélectionner une matière première pour chaque ligne de la recette.");
                return false;
            }

            if (_recipeRows.Count == 0)
            {
                var result = MessageBox.Show(
                    "Aucune recette définie. Voulez-vous continuer?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                return result == MessageBoxResult.Yes;
            }

            return true;
        }

        #endregion

        #region Helper Methods

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

        private bool IsProductCodeDuplicate(string code, int? excludeProductId = null)
        {
            var query = _db.Products.Where(p => p.CodeProduit == code);
            if (excludeProductId.HasValue)
                query = query.Where(p => p.Id != excludeProductId.Value);
            return query.Any();
        }

        private void UpdateStatistics()
        {
            try
            {
                int totalProducts = _products.Count;
                decimal averageCost = totalProducts > 0 ? _products.Average(p => p.CoutProduction) : 0;
                decimal totalValue = _products.Sum(p => p.PrixVente);

                if (TotalProductsText != null)
                    TotalProductsText.Text = $"📊 Total: {totalProducts} produits";

                if (AverageCostText != null)
                    AverageCostText.Text = $"💰 Coût Moyen: {averageCost:N2} DA";

                if (TotalValueText != null)
                    TotalValueText.Text = $"💵 Valeur Totale: {totalValue:N2} DA";
            }
            catch (Exception ex)
            {
                // Silent fail for statistics
            }
        }

        private void UpdateStatus(string message, Brush color)
        {
            if (StatusText != null)
            {
                StatusText.Text = message;
                StatusText.Foreground = color;
            }
        }

        private void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateStatus(message, Brushes.LimeGreen);
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            UpdateStatus(message, Brushes.Red);
        }

        #endregion
    }
}
