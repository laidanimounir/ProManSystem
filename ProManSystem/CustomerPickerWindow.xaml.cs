using Microsoft.EntityFrameworkCore;
using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProManSystem.Views
{
    public partial class CustomerPickerWindow : Window
    {
        private readonly AppDbContext _db = new AppDbContext();
        private List<Customer> _allCustomers = new();

        public Customer? SelectedCustomer { get; private set; } = null;

        public CustomerPickerWindow()
        {
            InitializeComponent();
            this.Loaded += CustomerPickerWindow_Loaded;
        }

        private void CustomerPickerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCustomers();
            SearchTextBox.Focus();
        }

        private void LoadCustomers(string? searchTerm = null)
        {
            try
            {
                IQueryable<Customer> query = _db.Customers;

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.Trim().ToLower();
                    query = query.Where(c =>
                        c.CodeClient.ToLower().Contains(searchTerm) ||
                        c.NomComplet.ToLower().Contains(searchTerm) ||
                        (c.Activite != null && c.Activite.ToLower().Contains(searchTerm)) ||
                        (c.Adresse != null && c.Adresse.ToLower().Contains(searchTerm)));
                }

                _allCustomers = query.OrderBy(c => c.CodeClient).ToList();
                CustomersGrid.ItemsSource = _allCustomers;

              
                if (CountTextBlock != null)
                {
                    CountTextBlock.Text = $"{_allCustomers.Count} client(s) trouvé(s)";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de chargement des clients: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchTextBox.Text?.Trim() ?? "";
            LoadCustomers(string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm);
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomersGrid.SelectedItem is Customer selected)
            {
                SelectedCustomer = selected;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un client.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedCustomer = null;
            this.DialogResult = false;
            this.Close();
        }

        private void CustomersGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CustomersGrid.SelectedItem is Customer selected)
            {
                SelectedCustomer = selected;
                this.DialogResult = true;
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _db?.Dispose();
            base.OnClosed(e);
        }
    }
}
