using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace ProManSystem.Views
{
    public partial class PurchaseInvoiceEditWindow : Window
    {
        private readonly AppDbContext _db;
        private PurchaseInvoice _invoice;
        private ObservableCollection<PurchaseInvoiceLine> _lines;

        public PurchaseInvoiceEditWindow(int invoiceId, AppDbContext db)
        {
            InitializeComponent();
            _db = db;

            _invoice = _db.PurchaseInvoices
                .Where(i => i.Id == invoiceId)
                .FirstOrDefault();

            if (_invoice == null)
            {
                MessageBox.Show("الفاتورة غير موجودة.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            LoadInvoiceData();
        }

        private void LoadInvoiceData()
        {
            NumeroFactureTextBox.Text = _invoice.NumeroFacture;
            DateFactureDatePicker.SelectedDate = _invoice.DateFacture;
            SupplierTextBox.Text = _invoice.Supplier.Designation;

            _lines = new ObservableCollection<PurchaseInvoiceLine>(_invoice.Lignes);
            PurchaseInvoicesGrid.ItemsSource = _lines;


            UpdateTotals();
        }

        private void UpdateTotals()
        {
            decimal totalHT = _lines.Sum(l => l.MontantLigne);
            decimal tva = totalHT * 0.19m;
            decimal totalTTC = totalHT + tva;

            TotalHTTextBox.Text = totalHT.ToString("F2");
            TVATextBox.Text = tva.ToString("F2");
            TotalTTCTextBox.Text = totalTTC.ToString("F2");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _invoice.NumeroFacture = NumeroFactureTextBox.Text;
                _invoice.DateFacture = DateFactureDatePicker.SelectedDate ?? DateTime.Now;
                _invoice.MontantHT = decimal.Parse(TotalHTTextBox.Text);
                _invoice.MontantTVA = decimal.Parse(TVATextBox.Text);
                _invoice.MontantTTC = decimal.Parse(TotalTTCTextBox.Text);


                _invoice.Lignes.Clear();
                foreach (var line in _lines)
                {
                    line.MontantLigne = line.Quantite * line.PrixUnitaire;
                    _invoice.Lignes.Add(line);
                }


                _db.SaveChanges();

                MessageBox.Show("تم تحديث الفاتورة بنجاح.", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ: " + ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void LoadInvoices()
        {
            //  تحميل الفواتير لاحقاً
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
