using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProManSystem.Views
{
    public partial class PurchaseInvoicesView : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();

        private Supplier? _selectedSupplier;
        private ObservableCollection<PurchaseInvoiceLine> _lines = new();
        private ObservableCollection<PurchaseInvoice> _history = new();

        public PurchaseInvoicesView()
        {
            InitializeComponent();
            InitTvaList();
            LoadHistory();
            PrepareNewInvoice();

            LinesGrid.ItemsSource = _lines;
            HistoryGrid.ItemsSource = _history;
        }

        

        private void InitTvaList()
        {
           
            var defaultRates = new[] { 19m, 9m, 0m };

            foreach (var r in defaultRates)
                TvaComboBox.Items.Add(r.ToString("0.##"));

            TvaComboBox.Text = "19"; 
        }

        private decimal GetTvaRate()
        {
            var txt = (TvaComboBox.Text ?? "0").Replace('.', ',');
            return decimal.TryParse(txt, out var t) ? t : 0m;
        }

        private void SaveTvaButton_Click(object sender, RoutedEventArgs e)
        {
            var txt = (TvaComboBox.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(txt))
                return;

            if (!TvaComboBox.Items.Cast<object>().Any(i => i.ToString() == txt))
                TvaComboBox.Items.Add(txt);

            MessageBox.Show("تم حفظ نسبة TVA في القائمة.", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

      

        private void PrepareNewInvoice()
        {
            NumeroFactureTextBox.Text = GenerateInvoiceNumber();
            DateFacturePicker.SelectedDate = DateTime.Today;
            _selectedSupplier = null;
            SupplierTextBox.Text = "";

            _lines.Clear();

            MontantHTTextBox.Text = "0.00";
            MontantTVATextBox.Text = "0.00";
            MontantTTCTextBox.Text = "0.00";
           
           
        }

        private string GenerateInvoiceNumber()
        {
            try
            {
                var last = _db.PurchaseInvoices
                    .OrderByDescending(f => f.Id)
                    .Select(f => f.NumeroFacture)
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(last))
                    return "FA001";

                if (!last.StartsWith("FA") || last.Length < 3)
                    return "FA001";

                if (!int.TryParse(last.Substring(2), out int num))
                    return "FA001";

                return "FA" + (num + 1).ToString("D3");
            }
            catch
            {
                return "FA001";
            }
        }

       

        private void PickSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new SupplierPickerWindow
            {
                Owner = Application.Current.MainWindow
            };

            if (win.ShowDialog() == true && win.SelectedSupplier != null)
            {
                _selectedSupplier = win.SelectedSupplier;
                SupplierTextBox.Text = $"{_selectedSupplier.CodeFournisseur} - {_selectedSupplier.Designation}";
            }
        }

      

        private void AddLineButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PurchaseLineDialog(); 
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true && dialog.CreatedLine != null)
            {
                _lines.Add(dialog.CreatedLine);
                RecalculateTotals();
            }
        }

        private void DeleteLineButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseInvoiceLine line)
            {
                _lines.Remove(line);
                RecalculateTotals();
            }
        }

        private void RecalculateTotals()
        {
            decimal ht = _lines.Sum(l => l.MontantLigne);
            decimal tvaRate = GetTvaRate() / 100m;
            decimal tva = Math.Round(ht * tvaRate, 2);
            decimal ttc = ht + tva;

            MontantHTTextBox.Text = ht.ToString("0.00");
            MontantTVATextBox.Text = tva.ToString("0.00");
            MontantTTCTextBox.Text = ttc.ToString("0.00");

         
            
        }

      



        private void SaveInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSupplier == null)
            {
                MessageBox.Show("اختر المورد أولاً.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_lines.Count == 0)
            {
                MessageBox.Show("أضف على الأقل سطراً واحداً للفاتورة.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DateFacturePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("حدد تاريخ الفاتورة.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
            decimal ht = decimal.Parse(MontantHTTextBox.Text.Replace('.', ','));
            decimal tva = decimal.Parse(MontantTVATextBox.Text.Replace('.', ','));
            decimal ttc = decimal.Parse(MontantTTCTextBox.Text.Replace('.', ','));
            
            
            decimal tvaRate = GetTvaRate();

            try
            {
                var invoice = new PurchaseInvoice
                {
                    NumeroFacture = NumeroFactureTextBox.Text,
                    SupplierId = _selectedSupplier.Id,
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





                foreach (var line in _lines)
                {
                    invoice.Lignes.Add(new PurchaseInvoiceLine
                    {
                        RawMaterialId = line.RawMaterialId,
                        Quantite = line.Quantite,
                        PrixUnitaire = line.PrixUnitaire,
                        MontantLigne = line.MontantLigne
                    });
                }

                _db.PurchaseInvoices.Add(invoice);

               
                foreach (var l in invoice.Lignes)
                    UpdateStockAndPmapa(l.RawMaterialId, l.Quantite, l.PrixUnitaire);

               
                var supplier = _db.Suppliers.First(s => s.Id == invoice.SupplierId);
                supplier.TotalAchats = (supplier.TotalAchats ?? 0) + invoice.MontantTTC;
                supplier.Dette = (supplier.Dette ?? 0) + invoice.Reste;

                _db.SaveChanges();

                _history.Add(invoice);
                HistoryGrid.Items.Refresh();

                MessageBox.Show("تم حفظ فاتورة الشراء بنجاح.",
                    "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                PrepareNewInvoice();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'enregistrement : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStockAndPmapa(int rawMaterialId, decimal qteAchetee, decimal prixAchat)
        {
            var mat = _db.RawMaterials.First(m => m.Id == rawMaterialId);

            decimal stockAncien = mat.StockActuel;
            decimal pmapaAncien = mat.PMAPA;

            decimal stockNouveau = stockAncien + qteAchetee;

            if (stockNouveau <= 0)
            {
                mat.StockActuel = stockNouveau;
               
                return;
            }

            decimal valeurAncienne = stockAncien * pmapaAncien;
            decimal valeurNouvelle = qteAchetee * prixAchat;
            decimal pmapaNouveau = (valeurAncienne + valeurNouvelle) / stockNouveau;

            mat.StockActuel = stockNouveau;
            mat.PMAPA = Math.Round(pmapaNouveau, 4);
        }

       

        private void LoadHistory()
        {
            _history = new ObservableCollection<PurchaseInvoice>(
                _db.PurchaseInvoices
                   .OrderByDescending(f => f.DateFacture)
                   .Take(100)
                   .ToList()
            );
        }

        private void HistorySearchButton_Click(object sender, RoutedEventArgs e)
        {
            string term = (HistorySearchTextBox.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(term))
            {
                HistoryGrid.ItemsSource = _history;
                return;
            }

            var res = _history
                .Where(f =>
                    f.NumeroFacture.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (f.Supplier != null &&
                     f.Supplier.Designation.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            HistoryGrid.ItemsSource = res;
        }

        private void HistoryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
