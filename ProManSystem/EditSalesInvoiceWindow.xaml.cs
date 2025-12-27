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
    public partial class EditSalesInvoiceWindow : Window
    {
        private readonly AppDbContext _db = new AppDbContext();
        private SalesInvoice _originalInvoice;
        private ObservableCollection<SalesInvoiceLine> _invoiceLines = new();
        private ObservableCollection<Product> _availableProducts = new();
        private List<SalesInvoiceLine> _deletedLines = new();
        private Dictionary<int, decimal> _originalQuantities = new();
        private bool _isCalculating = false;

        public bool WasSaved { get; private set; } = false;

        public EditSalesInvoiceWindow(int invoiceId)
        {
            InitializeComponent();
            LoadInvoice(invoiceId);
            this.Loaded += EditSalesInvoiceWindow_Loaded;
        }

        private void EditSalesInvoiceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitTvaList();
            LoadAvailableProducts();
            InvoiceLinesGrid.ItemsSource = _invoiceLines;
            AvailableProductsGrid.ItemsSource = _availableProducts;
        }

        #region Initialization

        private void InitTvaList()
        {
            var defaultRates = new[] { 19m, 9m, 0m };
            foreach (var rate in defaultRates)
                TvaComboBox.Items.Add(rate.ToString("0.##"));
        }

        private decimal GetTvaRate()
        {
            var txt = (TvaComboBox.Text ?? "0").Replace('.', ',');
            return decimal.TryParse(txt, out var t) ? t : 0m;
        }

        private void LoadInvoice(int invoiceId)
        {
            try
            {
                _originalInvoice = _db.SalesInvoices
                    .Include(i => i.Customer)
                    .Include(i => i.Lignes)
                    .ThenInclude(l => l.Product)
                    .ThenInclude(p => p.ProductRecipes)
                    .ThenInclude(pr => pr.RawMaterial)
                    .FirstOrDefault(i => i.Id == invoiceId);

                if (_originalInvoice == null)
                {
                    MessageBox.Show("Facture introuvable!", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                // Load invoice info
                NumeroFactureTextBox.Text = _originalInvoice.NumeroFacture;
                DateFacturePicker.SelectedDate = _originalInvoice.DateFacture;
                CustomerTextBox.Text = $"{_originalInvoice.Customer.CodeClient} - {_originalInvoice.Customer.NomComplet}";
                TvaComboBox.Text = _originalInvoice.TauxTVA.ToString("0.##");

                // Load lines
                foreach (var line in _originalInvoice.Lignes)
                {
                    var newLine = new SalesInvoiceLine
                    {
                        Id = line.Id,
                        ProductId = line.ProductId,
                        Product = line.Product,
                        Quantite = line.Quantite,
                        PrixUnitaire = line.PrixUnitaire,
                        MontantLigne = line.MontantLigne
                    };
                    _invoiceLines.Add(newLine);
                    _originalQuantities[line.Id] = line.Quantite;
                }

                // Load remise
                if (_originalInvoice.TypeRemise == RemiseType.Pourcentage)
                {
                    RemisePourcentageRadio.IsChecked = true;
                    RemisePourcentageTextBox.Text = _originalInvoice.RemiseValeur.ToString("0.##");
                }
                else if (_originalInvoice.TypeRemise == RemiseType.Fixe)
                {
                    RemiseFixeRadio.IsChecked = true;
                    RemiseFixeTextBox.Text = _originalInvoice.RemiseValeur.ToString("0.##");
                }

                // Load mode reglement
                switch (_originalInvoice.ModeReglement)
                {
                    case ModeReglement.Espece:
                        ReglementEspeceRadio.IsChecked = true;
                        break;
                    case ModeReglement.VersementBancaire:
                        ReglementBancaireRadio.IsChecked = true;
                        break;
                    case ModeReglement.ATerme:
                        ReglementTermeRadio.IsChecked = true;
                        break;
                    case ModeReglement.Mixte:
                        ReglementMixteRadio.IsChecked = true;
                        break;
                }

                // Set old TTC for comparison
                OldTTCTextBlock.Text = _originalInvoice.MontantTTC.ToString("N2") + " DA";

                // Update header
                HeaderTextBlock.Text = $"Modifier la Facture {_originalInvoice.NumeroFacture}";

                RecalculateTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de chargement: {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

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
                    _availableProducts.Add(product);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de chargement des produits: {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Line Management

        private void InvoiceLinesGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (InvoiceLinesGrid.SelectedItem is not SalesInvoiceLine selectedLine)
                return;

            var dialog = new QuantityEditDialog(
                selectedLine.Product.Nom,
                selectedLine.Quantite,
                selectedLine.PrixUnitaire)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                selectedLine.Quantite = dialog.EnteredQuantity;
                selectedLine.PrixUnitaire = dialog.EnteredPrice;
                selectedLine.MontantLigne = selectedLine.Quantite * selectedLine.PrixUnitaire;

                InvoiceLinesGrid.Items.Refresh();
                RecalculateTotals();
            }
        }

        private void DeleteLineButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not SalesInvoiceLine line)
                return;

            var result = MessageBox.Show(
                $"Voulez-vous vraiment supprimer ce produit?\n\n{line.Product.Nom}",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _deletedLines.Add(line);
                _invoiceLines.Remove(line);
                RecalculateTotals();
            }
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Astuce: Double-cliquez sur un produit dans la liste 'Produits Disponibles' pour l'ajouter rapidement!",
                "Information",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void AvailableProductsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AvailableProductsGrid.SelectedItem is not Product selectedProduct)
                return;

            var dialog = new QuantityEditDialog(
                selectedProduct.Nom,
                1,
                selectedProduct.PrixVente)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                var newLine = new SalesInvoiceLine
                {
                    ProductId = selectedProduct.Id,
                    Product = selectedProduct,
                    Quantite = dialog.EnteredQuantity,
                    PrixUnitaire = dialog.EnteredPrice,
                    MontantLigne = dialog.EnteredQuantity * dialog.EnteredPrice
                };

                _invoiceLines.Add(newLine);
                RecalculateTotals();
            }
        }

        #endregion

        #region Calculations

        private void RecalculateTotals()
        {
            if (_isCalculating) return;
            _isCalculating = true;

            try
            {
                // Calculate Total HT
                decimal totalHT = _invoiceLines.Sum(l => l.MontantLigne);

                if (MontantHTTextBox != null)
                    MontantHTTextBox.Text = totalHT.ToString("N2");

                // Calculate Remise
                decimal remiseMontant = CalculateRemiseMontant(totalHT);

                if (RemiseMontantTextBlock != null)
                    RemiseMontantTextBlock.Text = remiseMontant.ToString("N2") + " DA";

                // Calculate Net HT
                decimal netHT = totalHT - remiseMontant;

                if (NetHTTextBox != null)
                    NetHTTextBox.Text = netHT.ToString("N2");

                // Calculate TVA
                decimal tvaRate = GetTvaRate() / 100m;
                decimal tva = Math.Round(netHT * tvaRate, 2);

                if (MontantTVATextBox != null)
                    MontantTVATextBox.Text = tva.ToString("N2");

                // Calculate Total TTC
                decimal ttc = netHT + tva;

                if (MontantTTCTextBox != null)
                    MontantTTCTextBox.Text = ttc.ToString("N2");

                // Update comparison
                if (NewTTCTextBlock != null)
                    NewTTCTextBlock.Text = ttc.ToString("N2") + " DA";

                if (DifferenceTTCTextBlock != null)
                {
                    decimal difference = ttc - _originalInvoice.MontantTTC;
                    DifferenceTTCTextBlock.Text = difference.ToString("+0.00;-0.00;0.00") + " DA";
                    DifferenceTTCTextBlock.Foreground = difference >= 0
                        ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(16, 185, 129))
                        : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(239, 68, 68));
                }
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

        private void TvaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RecalculateTotals();
        }

        #endregion

        #region Stock Management

        private void UpdateStockForModifications()
        {
            // Process deleted lines - return stock
            foreach (var deletedLine in _deletedLines)
            {
                if (deletedLine.Id > 0) // Only if it was an original line
                {
                    ReturnProductToStock(deletedLine.Product, deletedLine.Quantite);
                }
            }

            // Process modified and new lines
            foreach (var line in _invoiceLines)
            {
                if (line.Id > 0) // Existing line - check if modified
                {
                    if (_originalQuantities.ContainsKey(line.Id))
                    {
                        decimal originalQty = _originalQuantities[line.Id];
                        decimal newQty = line.Quantite;
                        decimal difference = newQty - originalQty;

                        if (difference != 0)
                        {
                            if (difference > 0)
                            {
                                // Increased quantity - deduct more from stock
                                DeductProductFromStock(line.Product, difference);
                            }
                            else
                            {
                                // Decreased quantity - return to stock
                                ReturnProductToStock(line.Product, Math.Abs(difference));
                            }
                        }
                    }
                }
                else // New line - deduct from stock
                {
                    DeductProductFromStock(line.Product, line.Quantite);
                }
            }
        }

        private void DeductProductFromStock(Product product, decimal quantity)
        {
            var productWithRecipes = _db.Products
                .Include(p => p.ProductRecipes)
                .ThenInclude(pr => pr.RawMaterial)
                .FirstOrDefault(p => p.Id == product.Id);

            if (productWithRecipes == null || productWithRecipes.ProductRecipes == null)
                return;

            foreach (var recipe in productWithRecipes.ProductRecipes)
            {
                if (recipe.RawMaterial == null) continue;

                decimal requiredQty = quantity * recipe.QuantiteNecessaire;
                recipe.RawMaterial.StockActuel -= requiredQty;

                // Warning if negative
                if (recipe.RawMaterial.StockActuel < 0)
                {
                    MessageBox.Show(
                        $"⚠️ ATTENTION: Le stock de '{recipe.RawMaterial.Designation}' est maintenant négatif: {recipe.RawMaterial.StockActuel:N2}",
                        "Stock Négatif",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }

            // Update product stock
            decimal newProductStock = CalculateMaxQuantityFromRawMaterials(productWithRecipes);
            productWithRecipes.StockActuel = newProductStock;
        }

        private void ReturnProductToStock(Product product, decimal quantity)
        {
            var productWithRecipes = _db.Products
                .Include(p => p.ProductRecipes)
                .ThenInclude(pr => pr.RawMaterial)
                .FirstOrDefault(p => p.Id == product.Id);

            if (productWithRecipes == null || productWithRecipes.ProductRecipes == null)
                return;

            foreach (var recipe in productWithRecipes.ProductRecipes)
            {
                if (recipe.RawMaterial == null) continue;

                decimal returnQty = quantity * recipe.QuantiteNecessaire;
                recipe.RawMaterial.StockActuel += returnQty;
            }

            // Update product stock
            decimal newProductStock = CalculateMaxQuantityFromRawMaterials(productWithRecipes);
            productWithRecipes.StockActuel = newProductStock;
        }

        private decimal CalculateMaxQuantityFromRawMaterials(Product product)
        {
            if (product.ProductRecipes == null || !product.ProductRecipes.Any())
                return 0m;

            var validRecipes = product.ProductRecipes
                .Where(r => r.RawMaterial != null && r.QuantiteNecessaire > 0)
                .ToList();

            if (!validRecipes.Any())
                return 0m;

            var possibleQuantities = validRecipes
                .Select(r => r.RawMaterial.StockActuel / r.QuantiteNecessaire);

            var maxQty = possibleQuantities.Min();
            return Math.Floor(maxQty);
        }

        #endregion

        #region Save

        private void SaveInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_invoiceLines.Count == 0)
            {
                MessageBox.Show("La facture doit contenir au moins un article.", "Attention",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DateFacturePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Veuillez sélectionner la date de la facture.", "Attention",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Voulez-vous enregistrer les modifications?\n\n" +
                "Cette action mettra à jour:\n" +
                "• Le stock des matières premières\n" +
                "• Le CA du client\n" +
                "• Les totaux de la facture",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                // Parse values
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

                // Determine Remise Type
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

                // Determine Mode de Règlement
                ModeReglement modeReglement = ModeReglement.Espece;
                if (ReglementBancaireRadio.IsChecked == true)
                    modeReglement = ModeReglement.VersementBancaire;
                else if (ReglementTermeRadio.IsChecked == true)
                    modeReglement = ModeReglement.ATerme;
                else if (ReglementMixteRadio.IsChecked == true)
                    modeReglement = ModeReglement.Mixte;

                // Update stock first
                UpdateStockForModifications();

                // Update invoice
                _originalInvoice.DateFacture = DateFacturePicker.SelectedDate.Value;
                _originalInvoice.TypeRemise = remiseType;
                _originalInvoice.RemiseValeur = remiseValeur;
                _originalInvoice.RemiseMontant = remiseMontant;
                _originalInvoice.MontantHT = totalHT;
                _originalInvoice.NetHT = netHT;
                _originalInvoice.TauxTVA = tvaRate;
                _originalInvoice.MontantTVA = tva;
                _originalInvoice.MontantTTC = ttc;
                _originalInvoice.ModeReglement = modeReglement;

                // Remove deleted lines
                foreach (var deletedLine in _deletedLines)
                {
                    if (deletedLine.Id > 0)
                    {
                        _db.SalesInvoiceLines.Remove(deletedLine);
                    }
                }

                // Update existing lines and add new ones
                foreach (var line in _invoiceLines)
                {
                    if (line.Id > 0) // Existing line
                    {
                        var existingLine = _db.SalesInvoiceLines.Find(line.Id);
                        if (existingLine != null)
                        {
                            existingLine.Quantite = line.Quantite;
                            existingLine.PrixUnitaire = line.PrixUnitaire;
                            existingLine.MontantLigne = line.MontantLigne;
                        }
                    }
                    else // New line
                    {
                        _originalInvoice.Lignes.Add(new SalesInvoiceLine
                        {
                            ProductId = line.ProductId,
                            Quantite = line.Quantite,
                            PrixUnitaire = line.PrixUnitaire,
                            MontantLigne = line.MontantLigne
                        });
                    }
                }

                // Update Customer CA
                var customer = _db.Customers.First(c => c.Id == _originalInvoice.CustomerId);
                decimal oldTTC = _originalInvoice.MontantTTC;
                customer.CA_TTC = (customer.CA_TTC ?? 0) - oldTTC + ttc;

                _db.SaveChanges();

                MessageBox.Show("Facture modifiée avec succès!", "Succès",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                WasSaved = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'enregistrement: {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Voulez-vous annuler les modifications?\n\nToutes les modifications seront perdues.",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                WasSaved = false;
                this.Close();
            }
        }

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            _db?.Dispose();
            base.OnClosed(e);
        }
    }
}
