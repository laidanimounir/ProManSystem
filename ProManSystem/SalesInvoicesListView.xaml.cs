using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace ProManSystem.Views
{
    public partial class SalesInvoicesListView : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();
        private readonly ObservableCollection<SalesInvoiceListItem> _invoices = new();

        public SalesInvoicesListView()
        {
            InitializeComponent();
            InvoicesGrid.ItemsSource = _invoices;
            this.Loaded += SalesInvoicesListView_Loaded;
        }

        private void SalesInvoicesListView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadInvoices();
        }

        
        private void LoadInvoices()
        {
            try
            {
                _invoices.Clear();

                var query = _db.SalesInvoices
                    .Include(i => i.Customer)
                    .OrderByDescending(i => i.DateFacture)
                    .ThenByDescending(i => i.Id)
                    .ToList();

                foreach (var inv in query)
                {
                    _invoices.Add(new SalesInvoiceListItem
                    {
                        Id = inv.Id,
                        NumeroFacture = inv.NumeroFacture,
                        DateFacture = inv.DateFacture,
                        CustomerName = inv.Customer != null
                            ? $"{inv.Customer.CodeClient} - {inv.Customer.NomComplet}"
                            : string.Empty,
                        MontantTTC = inv.MontantTTC,
                        PaymentStatus = inv.EstPayee ? "مدفوعة" : "غير مدفوعة"
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ عند تحميل الفواتير: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

     
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string num = (SearchNumeroTextBox.Text ?? "").Trim();
                string cust = (SearchCustomerTextBox.Text ?? "").Trim();
                DateTime? from = FromDatePicker.SelectedDate;
                DateTime? to = ToDatePicker.SelectedDate;

                var query = _db.SalesInvoices
                    .Include(i => i.Customer)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(num))
                    query = query.Where(i => i.NumeroFacture.Contains(num));

                if (!string.IsNullOrWhiteSpace(cust))
                    query = query.Where(i =>
                        i.Customer != null &&
                        ((i.Customer.CodeClient ?? "").Contains(cust) ||
                         (i.Customer.NomComplet ?? "").Contains(cust)));

                if (from.HasValue)
                    query = query.Where(i => i.DateFacture >= from.Value.Date);

                if (to.HasValue)
                {
                    var toDate = to.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(i => i.DateFacture <= toDate);
                }

                var list = query
                    .OrderByDescending(i => i.DateFacture)
                    .ThenByDescending(i => i.Id)
                    .ToList();

                _invoices.Clear();
                foreach (var inv in list)
                {
                    _invoices.Add(new SalesInvoiceListItem
                    {
                        Id = inv.Id,
                        NumeroFacture = inv.NumeroFacture,
                        DateFacture = inv.DateFacture,
                        CustomerName = inv.Customer != null
                            ? $"{inv.Customer.CodeClient} - {inv.Customer.NomComplet}"
                            : string.Empty,
                        MontantTTC = inv.MontantTTC,
                        PaymentStatus = inv.EstPayee ? "مدفوعة" : "غير مدفوعة"
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في البحث: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchNumeroTextBox.Text = "";
            SearchCustomerTextBox.Text = "";
            FromDatePicker.SelectedDate = null;
            ToDatePicker.SelectedDate = null;
            LoadInvoices();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadInvoices();
        }

        private SalesInvoiceListItem? GetSelectedInvoice()
        {
            return InvoicesGrid.SelectedItem as SalesInvoiceListItem;
        }

        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSelectedInvoiceDetails();
        }

        private void InvoicesGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowSelectedInvoiceDetails();
        }

        private void ShowSelectedInvoiceDetails()
        {
            var item = GetSelectedInvoice();
            if (item == null)
            {
                MessageBox.Show("اختر فاتورة أولاً.",
                    "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

        
            MessageBox.Show($"رقم الفاتورة: {item.NumeroFacture}\n" +
                            $"الزبون: {item.CustomerName}\n" +
                            $"التاريخ: {item.DateFacture:dd/MM/yyyy}\n" +
                            $"المجموع TTC: {item.MontantTTC:N2}",
                            "تفاصيل الفاتورة",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
    }


    public class SalesInvoiceListItem
    {
        public int Id { get; set; }
        public string NumeroFacture { get; set; } = string.Empty;
        public DateTime DateFacture { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal MontantTTC { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
    }
}
