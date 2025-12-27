using Microsoft.EntityFrameworkCore;
using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProManSystem.Views
{
    public partial class SalesInvoicesListView : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();
        private ObservableCollection<SalesInvoiceViewModel> _invoices = new();

        public SalesInvoicesListView()
        {
            InitializeComponent();
            this.Loaded += SalesInvoicesListView_Loaded;
        }

        private void SalesInvoicesListView_Loaded(object sender, RoutedEventArgs e)
        {
            InvoicesGrid.ItemsSource = _invoices;
            LoadInvoices();
            LoadStatistics();
        }

        #region Load Data

        private void LoadInvoices(string searchTerm = null, DateTime? dateFrom = null,
            DateTime? dateTo = null, ModeReglement? modeFilter = null)
        {
            try
            {
                IQueryable<SalesInvoice> query = _db.SalesInvoices
                    .Include(i => i.Customer)
                    .Include(i => i.Lignes);

           
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.Trim().ToLower();
                    query = query.Where(i =>
                        i.NumeroFacture.ToLower().Contains(searchTerm) ||
                        i.Customer.NomComplet.ToLower().Contains(searchTerm) ||
                        i.Customer.CodeClient.ToLower().Contains(searchTerm));
                }

                if (dateFrom.HasValue)
                {
                    query = query.Where(i => i.DateFacture >= dateFrom.Value);
                }

                if (dateTo.HasValue)
                {
                    query = query.Where(i => i.DateFacture <= dateTo.Value);
                }

                if (modeFilter.HasValue)
                {
                    query = query.Where(i => i.ModeReglement == modeFilter.Value);
                }

                var invoices = query.OrderByDescending(i => i.DateFacture).ToList();

                _invoices.Clear();
                foreach (var invoice in invoices)
                {
                    _invoices.Add(new SalesInvoiceViewModel
                    {
                        Id = invoice.Id,
                        NumeroFacture = invoice.NumeroFacture,
                        DateFacture = invoice.DateFacture,
                        Customer = invoice.Customer,
                        MontantHT = invoice.MontantHT,
                        RemiseMontant = invoice.RemiseMontant,
                        NetHT = invoice.NetHT,
                        MontantTVA = invoice.MontantTVA,
                        MontantTTC = invoice.MontantTTC,
                        ModeReglement = invoice.ModeReglement,
                        ModeReglementDisplay = GetModeReglementDisplay(invoice.ModeReglement)
                    });
                }

                if (ResultsCountTextBlock != null)
                {
                    ResultsCountTextBlock.Text = $"{_invoices.Count} facture(s) trouvée(s)";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de chargement: {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStatistics()
        {
            try
            {
                var invoices = _db.SalesInvoices.ToList();

              
                decimal totalCA = invoices.Sum(i => i.MontantTTC);
                if (TotalCATextBlock != null)
                    TotalCATextBlock.Text = totalCA.ToString("N2") + " DA";

                if (TotalInvoicesTextBlock != null)
                    TotalInvoicesTextBlock.Text = invoices.Count.ToString();

             
                var thisMonth = DateTime.Now;
                decimal thisMonthCA = invoices
                    .Where(i => i.DateFacture.Year == thisMonth.Year && i.DateFacture.Month == thisMonth.Month)
                    .Sum(i => i.MontantTTC);
                if (ThisMonthCATextBlock != null)
                    ThisMonthCATextBlock.Text = thisMonthCA.ToString("N2") + " DA";

             
                int uniqueClients = invoices.Select(i => i.CustomerId).Distinct().Count();
                if (UniqueClientsTextBlock != null)
                    UniqueClientsTextBlock.Text = uniqueClients.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de chargement des statistiques: {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetModeReglementDisplay(ModeReglement mode)
        {
            return mode switch
            {
                ModeReglement.Espece => "💵 Espèce",
                ModeReglement.VersementBancaire => "🏦 Bancaire",
                ModeReglement.ATerme => "📅 À Terme",
                ModeReglement.Mixte => "🔀 Mixte",
                _ => "Inconnu"
            };
        }

        #endregion

        #region Search and Filters

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchTextBox.Text?.Trim();
            DateTime? dateFrom = DateFromPicker.SelectedDate;
            DateTime? dateTo = DateToPicker.SelectedDate;

            ModeReglement? modeFilter = null;
            if (ModeReglementComboBox.SelectedIndex > 0)
            {
                modeFilter = (ModeReglement)(ModeReglementComboBox.SelectedIndex - 1);
            }

            LoadInvoices(searchTerm, dateFrom, dateTo, modeFilter);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            DateFromPicker.SelectedDate = null;
            DateToPicker.SelectedDate = null;
            ModeReglementComboBox.SelectedIndex = 0;
            LoadInvoices();
        }

        private void ModeReglementComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /// Auto-search on selection change
        }

        #endregion

        #region Actions

        private void NewInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
          
            MessageBox.Show("Navigation vers création de facture à implémenter", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not SalesInvoiceViewModel invoice)
                return;

            var editWindow = new EditSalesInvoiceWindow(invoice.Id)
            {
                Owner = Window.GetWindow(this)
            };

            if (editWindow.ShowDialog() == true || editWindow.WasSaved)
            {
                LoadInvoices();
                LoadStatistics();
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not SalesInvoiceViewModel invoice)
                return;

            MessageBox.Show($"Impression de la facture {invoice.NumeroFacture} à implémenter", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PdfButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not SalesInvoiceViewModel invoice)
                return;

            MessageBox.Show($"Export PDF de la facture {invoice.NumeroFacture} à implémenter", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export PDF de toutes les factures à implémenter", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        protected void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _db?.Dispose();
        }

        #region ViewModel

        public class SalesInvoiceViewModel
        {
            public int Id { get; set; }
            public string NumeroFacture { get; set; } = string.Empty;
            public DateTime DateFacture { get; set; }
            public Customer Customer { get; set; } = null!;
            public decimal MontantHT { get; set; }
            public decimal RemiseMontant { get; set; }
            public decimal NetHT { get; set; }
            public decimal MontantTVA { get; set; }
            public decimal MontantTTC { get; set; }
            public ModeReglement ModeReglement { get; set; }
            public string ModeReglementDisplay { get; set; } = string.Empty;
        }

        #endregion
    }
}
