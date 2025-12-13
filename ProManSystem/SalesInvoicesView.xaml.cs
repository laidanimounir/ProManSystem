using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProManSystem.Views
{
    public partial class SalesInvoicesView : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();

        private Customer? _selectedCustomer;
        private ObservableCollection<SalesInvoiceLine> _lines = new();
        private ObservableCollection<SalesInvoice> _history = new();

        public SalesInvoicesView()
        {
            InitializeComponent();
            InitTvaList();
            LoadHistory();
            PrepareNewInvoice();
            UpdateStatistics(); // ← جديد

            LinesGrid.ItemsSource = _lines;
            HistoryGrid.ItemsSource = _history;
        }

        /// <summary>
        /// تحديث الإحصائيات الشهرية
        /// </summary>
        private void UpdateStatistics()
        {
            try
            {
                var firstDayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                // إجمالي المبيعات هذا الشهر
                var monthSales = _db.SalesInvoices
                    .Where(f => f.DateFacture >= firstDayOfMonth && f.DateFacture <= lastDayOfMonth)
                    .Sum(f => (decimal?)f.MontantTTC) ?? 0m;

                // عدد الفواتير هذا الشهر
                var invoiceCount = _db.SalesInvoices
                    .Count(f => f.DateFacture >= firstDayOfMonth && f.DateFacture <= lastDayOfMonth);

                // الربح التقديري (يمكن تحسينه بناءً على منطق الربح الفعلي)
                var estimatedProfit = _db.SalesInvoices
                    .Where(f => f.DateFacture >= firstDayOfMonth && f.DateFacture <= lastDayOfMonth)
                    .SelectMany(f => f.Lignes)
                    .Sum(l => (decimal?)(l.MontantLigne * 0.2m)) ?? 0m; // افتراض 20% هامش ربح

                // تحديث الواجهة
                if (StatsMonthSales != null)
                    StatsMonthSales.Text = $"{monthSales:N2} DA";

                if (StatsProfit != null)
                    StatsProfit.Text = $"{estimatedProfit:N2} DA";

                if (StatsInvoiceCount != null)
                    StatsInvoiceCount.Text = invoiceCount.ToString();
            }
            catch (Exception ex)
            {
                // في حالة الخطأ، عرض 0
                if (StatsMonthSales != null) StatsMonthSales.Text = "0.00 DA";
                if (StatsProfit != null) StatsProfit.Text = "0.00 DA";
                if (StatsInvoiceCount != null) StatsInvoiceCount.Text = "0";
            }
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
            _selectedCustomer = null;
            CustomerTextBox.Text = "";

            _lines.Clear();

            MontantHTTextBox.Text = "0.00";
            MontantTVATextBox.Text = "0.00";
            MontantTTCTextBox.Text = "0.00";
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

        private void PickCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new CustomerPickerWindow
            {
                Owner = Application.Current.MainWindow
            };

            if (win.ShowDialog() == true && win.SelectedCustomer != null)
            {
                _selectedCustomer = win.SelectedCustomer;
                CustomerTextBox.Text = $"{_selectedCustomer.CodeClient} - {_selectedCustomer.NomComplet}";
            }
        }

        private void AddLineButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SalesLineDialog();
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true && dialog.CreatedLine != null)
            {
                _lines.Add(dialog.CreatedLine);
                RecalculateTotals();
            }
        }

        private void DeleteLineButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesInvoiceLine line)
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
            if (_selectedCustomer == null)
            {
                MessageBox.Show("اختر الزبون أولاً.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_lines.Count == 0)
            {
                MessageBox.Show("أضف على الأقل سطرًا واحدًا للفاتورة.",
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

                foreach (var line in _lines)
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

                // تحديث مخزون المنتجات
                foreach (var l in invoice.Lignes)
                {
                    var product = _db.Products.First(p => p.Id == l.ProductId);
                    product.StockActuel -= l.Quantite;
                }

                // تحديث CA للزبون
                var customer = _db.Customers.First(c => c.Id == invoice.CustomerId);
                customer.CA_TTC = (customer.CA_TTC ?? 0) + invoice.MontantTTC;

                _db.SaveChanges();

                // إعادة تحميل الفاتورة مع البيانات المرتبطة
                var savedInvoice = _db.SalesInvoices
                    .Where(f => f.Id == invoice.Id)
                    .Select(f => new SalesInvoice
                    {
                        Id = f.Id,
                        NumeroFacture = f.NumeroFacture,
                        CustomerId = f.CustomerId,
                        DateFacture = f.DateFacture,
                        MontantHT = f.MontantHT,
                        TauxTVA = f.TauxTVA,
                        MontantTVA = f.MontantTVA,
                        MontantTTC = f.MontantTTC,
                        MontantPaye = f.MontantPaye,
                        Reste = f.Reste,
                        EstPayee = f.EstPayee,
                        Customer = f.Customer
                    })
                    .FirstOrDefault();

                if (savedInvoice != null)
                {
                    _history.Insert(0, savedInvoice);
                    HistoryGrid.Items.Refresh();
                }

                // تحديث الإحصائيات
                UpdateStatistics();

                MessageBox.Show("تم حفظ فاتورة البيع بنجاح.",
                    "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                PrepareNewInvoice();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'enregistrement : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadHistory()
        {
            try
            {
                var invoices = _db.SalesInvoices
                    .OrderByDescending(f => f.DateFacture)
                    .Take(100)
                    .Select(f => new SalesInvoice
                    {
                        Id = f.Id,
                        NumeroFacture = f.NumeroFacture,
                        CustomerId = f.CustomerId,
                        DateFacture = f.DateFacture,
                        MontantHT = f.MontantHT,
                        TauxTVA = f.TauxTVA,
                        MontantTVA = f.MontantTVA,
                        MontantTTC = f.MontantTTC,
                        MontantPaye = f.MontantPaye,
                        Reste = f.Reste,
                        EstPayee = f.EstPayee,
                        Customer = f.Customer
                    })
                    .ToList();

                _history = new ObservableCollection<SalesInvoice>(invoices);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement de l'historique : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                _history = new ObservableCollection<SalesInvoice>();
            }
        }

        private void HistorySearchButton_Click(object sender, RoutedEventArgs e)
        {
            string term = (HistorySearchTextBox.Text ?? "").Trim();

            // إزالة النص التوضيحي
            if (term.StartsWith("🔍"))
                term = term.Replace("🔍 Rechercher par N°, client, date...", "").Trim();

            if (string.IsNullOrWhiteSpace(term))
            {
                HistoryGrid.ItemsSource = _history;
                return;
            }

            var res = _history
                .Where(f =>
                    f.NumeroFacture.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (f.Customer != null &&
                     f.Customer.NomComplet.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            HistoryGrid.ItemsSource = res;
        }

        private void HistoryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // يمكن إضافة منطق لعرض تفاصيل الفاتورة المحددة
        }
    }
}
