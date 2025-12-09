using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ProManSystem.Data;
using ProManSystem.Models;

namespace ProManSystem
{
    public partial class CustomerSearchWindow : Window
    {
        private readonly AppDbContext _db = new AppDbContext();

        public CustomerSearchWindow()
        {
            InitializeComponent();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string term = SearchTextBox.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(term))
            {
                MessageBox.Show("Enter code or name to search.");
                return;
            }

            List<Customer> results = _db.Customers
                .Where(c =>
                    c.CodeClient.Contains(term) ||
                    c.NomComplet.Contains(term))
                .OrderBy(c => c.CodeClient)
                .ToList();

            ResultsGrid.ItemsSource = results;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var menu = new CustomerMenuWindow();
            menu.Show();
            this.Close();
        }
    }
}
