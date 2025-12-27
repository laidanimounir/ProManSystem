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
        private bool _isCalculating = false;

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
                MessageBox.Show($"Erreur de chargement: {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Initialization

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

          
            if (RemisePourcentageRadio != null)
                RemisePourcentageRadio.IsChecked = false;

            if (RemiseFixeRadio != null)
                RemiseFixeRadio.IsChecked = false;

            if (RemisePourcentageTextBox != null)
                RemisePourcentageTextBox.Text = "0";

            if (RemiseFixeTextBox != null)
                RemiseFixeTextBox.Text = "0";

            if (RemiseMontantTextBlock != null)
                RemiseMontantTextBlock.Text = "0.00 DA";

           
            if (ReglementEspeceRadio != null)
                ReglementEspeceRadio.IsChecked = true;

            if (MontantHTTextBox != null) MontantHTTextBox.Text = "0.00";
            if (NetHTTextBox != null) NetHTTextBox.Text = "0.00";
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

        #endregion

        #region Products Management

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
                MessageBox.Show($"Erreur de chargement des produits: {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private decimal CalculateMaxQuantityFromRawMaterials(Product product)
        {
            if (product.ProductRecipes == null)
                return 0m;

            var validRecipes = product.ProductRecipes
                .Where(r => r.RawMaterial != null && r.QuantiteNecessaire > 0 && r.RawMaterial.StockActuel > 0)
                .ToList();

            if (!validRecipes.Any())
                return 0m;

            var possibleQuantities = validRecipes
                .Select(r => r.RawMaterial.StockActuel / r.QuantiteNecessaire);

            var maxQty = possibleQuantities.Min();
            return Math.Floor(maxQty);
        }

        private void AvailableProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AvailableProductsGrid.SelectedItem is not ProductWithMaxQuantity selectedProduct)
            {
                _selectedProductRecipe.Clear();
                if (RecipeHeaderTextBlock != null)
                    RecipeHeaderTextBlock.Text = "📋 Recette du Produit";
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
                    RecipeHeaderTextBlock.Text = $"📋 Recette: {product.Nom}";

                var recipes = _db.ProductRecipes
                    .Include(pr => pr.RawMaterial)
                    .Where(pr => pr.ProductId == product.Id)
                    .ToList();

                if (!recipes.Any())
                {
                    MessageBox.Show("Ce produit n'a pas de recette définie!", "Information",
                        MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show($"Erreur de chargement de la recette: {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AvailableProductsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AvailableProductsGrid.SelectedItem is not ProductWithMaxQuantity selectedProduct)
                return;

            if (selectedProduct.MaxQuantity <= 0)
            {
                MessageBox.Show("Impossible de vendre ce produit - stock de matières premières insuffisant!",
                    "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
                        $"La quantité demandée ({requestedQty}) est supérieure au maximum ({selectedProduct.MaxQuantity}).\n" +
                        $"Voulez-vous continuer? (Le stock sera négatif)",
                        "Avertissement",
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

        private void DeleteLineButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesInvoiceLine line)
            {
                _invoiceLines.Remove(line);
                RecalculateTotals();
            }
        }

        #endregion

        #region Calculations with Remise

        private void RecalculateTotals()
        {
            if (_isCalculating) return;
            _isCalculating = true;

            try
            {
                
                decimal totalHT = _invoiceLines.Sum(l => l.MontantLigne);

                if (MontantHTTextBox != null)
                    MontantHTTextBox.Text = totalHT.ToString("N2");

                
                decimal remiseMontant = CalculateRemiseMontant(totalHT);

                if (RemiseMontantTextBlock != null)
                    RemiseMontantTextBlock.Text = remiseMontant.ToString("N2") + " DA";

             
                decimal netHT = totalHT - remiseMontant;

                if (NetHTTextBox != null)
                    NetHTTextBox.Text = netHT.ToString("N2");

               
                decimal tvaRate = GetTvaRate() / 100m;
                decimal tva = Math.Round(netHT * tvaRate, 2);

                if (MontantTVATextBox != null)
                    MontantTVATextBox.Text = tva.ToString("N2");

               
                decimal ttc = netHT + tva;

                if (MontantTTCTextBox != null)
                    MontantTTCTextBox.Text = ttc.ToString("N2");
            }
            finally
            {
                _isCalculating = false;
            }
        }

        private decimal CalculateRemiseMontant(decimal totalHT)
        {
            if (totalHT <= 0)
                return 0m;

            if (RemisePourcentageRadio?.IsChecked == true)
            {
                if (decimal.TryParse(RemisePourcentageTextBox.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal percentage))
                {
                    return Math.Round(totalHT * (percentage / 100m), 2);
                }
            }
            else if (RemiseFixeRadio?.IsChecked == true)
            {
                if (decimal.TryParse(RemiseFixeTextBox.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal fixedAmount))
                {
                    return Math.Round(fixedAmount, 2);
                }
            }

            return 0m;
        }

        private void RemiseType_Changed(object sender, RoutedEventArgs e)
        {
            RecalculateTotals();
        }

        private void RemisePourcentageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isCalculating) return;
            if (RemisePourcentageRadio?.IsChecked == true)
            {
                RecalculateTotals();
            }
        }

        private void RemiseFixeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isCalculating) return;
            if (RemiseFixeRadio?.IsChecked == true)
            {
                RecalculateTotals();
            }
        }

        private void NetHT_Changed(object sender, TextChangedEventArgs e)
        {
            if (_isCalculating) return;

           
            if (MontantHTTextBox == null || NetHTTextBox == null || RemiseMontantTextBlock == null)
                return;

            if (!decimal.TryParse(MontantHTTextBox.Text.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal totalHT))
                return;

            if (!decimal.TryParse(NetHTTextBox.Text.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal netHT))
                return;

            if (totalHT <= 0)
                return;

            _isCalculating = true;

            try
            {
                decimal remiseMontant = totalHT - netHT;

                if (remiseMontant < 0)
                    remiseMontant = 0;

                RemiseMontantTextBlock.Text = remiseMontant.ToString("N2") + " DA";

                
                if (RemisePourcentageRadio?.IsChecked == true)
                {
                    decimal percentage = (remiseMontant / totalHT) * 100m;
                    RemisePourcentageTextBox.Text = percentage.ToString("N2");
                }
                else if (RemiseFixeRadio?.IsChecked == true)
                {
                    RemiseFixeTextBox.Text = remiseMontant.ToString("N2");
                }

              
                decimal tvaRate = GetTvaRate() / 100m;
                decimal tva = Math.Round(netHT * tvaRate, 2);
                decimal ttc = netHT + tva;

                if (MontantTVATextBox != null)
                    MontantTVATextBox.Text = tva.ToString("N2");

                if (MontantTTCTextBox != null)
                    MontantTTCTextBox.Text = ttc.ToString("N2");
            }
            finally
            {
                _isCalculating = false;
            }
        }

        private void TvaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RecalculateTotals();
        }

        #endregion

        #region Customer Selection

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

        #endregion

        #region Save Invoice

        private void SaveInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                MessageBox.Show("Veuillez sélectionner un client.", "Attention",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_invoiceLines.Count == 0)
            {
                MessageBox.Show("Veuillez ajouter des produits à la facture.", "Attention",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DateFacturePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Veuillez sélectionner la date de la facture.", "Attention",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
             
                decimal totalHT = decimal.Parse(MontantHTTextBox.Text.Replace(',', '.'),
                    System.Globalization.CultureInfo.InvariantCulture);

                decimal remiseMontant = decimal.Parse(RemiseMontantTextBlock.Text.Replace(" DA", "").Replace(',', '.'),
                    System.Globalization.CultureInfo.InvariantCulture);

                decimal netHT = decimal.Parse(NetHTTextBox.Text.Replace(',', '.'),
                    System.Globalization.CultureInfo.InvariantCulture);

                decimal tva = decimal.Parse(MontantTVATextBox.Text.Replace(',', '.'),
                    System.Globalization.CultureInfo.InvariantCulture);

                decimal ttc = decimal.Parse(MontantTTCTextBox.Text.Replace(',', '.'),
                    System.Globalization.CultureInfo.InvariantCulture);

                decimal tvaRate = GetTvaRate();

                RemiseType remiseType = RemiseType.Aucune;
                decimal remiseValeur = 0m;

                if (RemisePourcentageRadio.IsChecked == true)
                {
                    remiseType = RemiseType.Pourcentage;
                    decimal.TryParse(RemisePourcentageTextBox.Text.Replace(',', '.'),
                        System.Globalization.CultureInfo.InvariantCulture,
                        out remiseValeur);
                }
                else if (RemiseFixeRadio.IsChecked == true)
                {
                    remiseType = RemiseType.Fixe;
                    decimal.TryParse(RemiseFixeTextBox.Text.Replace(',', '.'),
                        System.Globalization.CultureInfo.InvariantCulture,
                        out remiseValeur);
                }

             
                ModeReglement modeReglement = ModeReglement.Espece;
                if (ReglementBancaireRadio.IsChecked == true)
                    modeReglement = ModeReglement.VersementBancaire;
                else if (ReglementTermeRadio.IsChecked == true)
                    modeReglement = ModeReglement.ATerme;
                else if (ReglementMixteRadio.IsChecked == true)
                    modeReglement = ModeReglement.Mixte;

               
                var invoice = new SalesInvoice
                {
                    NumeroFacture = NumeroFactureTextBox.Text,
                    CustomerId = _selectedCustomer.Id,
                    DateFacture = DateFacturePicker.SelectedDate.Value,
                    TypeRemise = remiseType,
                    RemiseValeur = remiseValeur,
                    RemiseMontant = remiseMontant,
                    MontantHT = totalHT,
                    NetHT = netHT,
                    TauxTVA = tvaRate,
                    MontantTVA = tva,
                    MontantTTC = ttc,
                    ModeReglement = modeReglement,
                    MontantPaye = ttc,
                    Reste = 0m,
                    EstPayee = true,
                    DateCreation = DateTime.Now
                };

              
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

                // update Raw Materials Stock
                foreach (var line in invoice.Lignes)
                {
                    var product = _db.Products
                        .Include(p => p.ProductRecipes)
                        .ThenInclude(pr => pr.RawMaterial)
                        .First(p => p.Id == line.ProductId);

                    foreach (var recipe in product.ProductRecipes)
                    {
                        var rawMaterial = recipe.RawMaterial;
                        decimal requiredQty = line.Quantite * recipe.QuantiteNecessaire;
                        rawMaterial.StockActuel -= requiredQty;
                    }

                    // update Product Stock
                    decimal newProductStock = CalculateMaxQuantityFromRawMaterials(product);
                    product.StockActuel = newProductStock;
                }

                // update Customer CA
                var customer = _db.Customers.First(c => c.Id == invoice.CustomerId);
                customer.CA_TTC = (customer.CA_TTC ?? 0) + invoice.MontantTTC;

                _db.SaveChanges();

                MessageBox.Show("Facture enregistrée avec succès!", "Succès",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                PrepareNewInvoice();
                LoadAvailableProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'enregistrement: {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Voulez-vous réinitialiser la facture?\n\nToutes les données non enregistrées seront perdues.",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                PrepareNewInvoice();
                LoadAvailableProducts();
            }
        }

        #endregion

        #region Helper Classes

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

        #endregion
    }
}
