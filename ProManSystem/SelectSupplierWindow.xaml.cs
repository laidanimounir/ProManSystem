using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProManSystem.Views
{
    public partial class SelectSupplierWindow : Window
    {
        private readonly AppDbContext _db = new AppDbContext();
        private ObservableCollection<Supplier> _suppliers = new();

        public Supplier? SelectedSupplier { get; private set; }

        public SelectSupplierWindow()
        {
            InitializeComponent();
            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            try
            {
                _suppliers = new ObservableCollection<Supplier>(
                    _db.Suppliers
                       .OrderBy(s => s.CodeFournisseur)
                       .ToList()
                );
                SuppliersGrid.ItemsSource = _suppliers;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des fournisseurs : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string term = (SearchTextBox.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(term))
            {
                SuppliersGrid.ItemsSource = _suppliers;
                return;
            }

            var results = _suppliers
                .Where(s =>
                    (!string.IsNullOrEmpty(s.CodeFournisseur) &&
                     s.CodeFournisseur.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(s.Designation) &&
                     s.Designation.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            SuppliersGrid.ItemsSource = results;
        }

        private void SelectCurrent()
        {
            var supplier = SuppliersGrid.SelectedItem as Supplier;
            if (supplier == null)
            {
                MessageBox.Show("Sélectionnez un fournisseur.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SelectedSupplier = supplier;
            DialogResult = true;
            Close();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectCurrent();
        }

        private void SuppliersGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
