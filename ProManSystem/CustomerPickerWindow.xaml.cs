using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProManSystem.Views
{
    public partial class CustomerPickerWindow : Window
    {
        private readonly AppDbContext _db = new AppDbContext();
        private List<Customer> _customers = new();

        public Customer? SelectedCustomer { get; private set; }

        public CustomerPickerWindow()
        {
            InitializeComponent();
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            try
            {
                _customers = _db.Customers
                    .Where(c => !c.EstRadie)
                    .OrderBy(c => c.CodeClient)
                    .ToList();

                CustomersGrid.ItemsSource = _customers;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des clients : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string term = (SearchTextBox.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(term))
            {
                CustomersGrid.ItemsSource = _customers;
                return;
            }

            var results = _customers
                .Where(c =>
                    (!string.IsNullOrEmpty(c.CodeClient) &&
                     c.CodeClient.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(c.NomComplet) &&
                     c.NomComplet.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            CustomersGrid.ItemsSource = results;
        }

        private void SelectCurrent()
        {
            var selected = CustomersGrid.SelectedItem as Customer;
            if (selected == null)
            {
                MessageBox.Show("Sélectionnez un client.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SelectedCustomer = selected;
            DialogResult = true;
            Close();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectCurrent();
        }

        private void CustomersGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectCurrent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
