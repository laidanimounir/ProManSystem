using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProManSystem.Views
{
    public partial class SuppliersEditorView : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();
        private ObservableCollection<Supplier> _suppliers = new ObservableCollection<Supplier>();
        private Supplier? _selectedSupplier;

        public SuppliersEditorView()
        {
            InitializeComponent();
            LoadSuppliers();
            PrepareNewSupplier();
            ManageSuppliersGrid.ItemsSource = _suppliers;
        }

        private void LoadSuppliers()
        {
            try
            {
                var list = _db.Suppliers.ToList();
                _suppliers = new ObservableCollection<Supplier>(list);
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
            string term = SearchTextBox.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(term))
            {
                SuppliersGrid.ItemsSource = _suppliers;
                return;
            }

            var results = _suppliers
                .Where(s =>
                    (!string.IsNullOrEmpty(s.CodeFournisseur) && s.CodeFournisseur.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(s.Designation) && s.Designation.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            SuppliersGrid.ItemsSource = results;
        }

        private void ManageSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string term = ManageSearchTextBox.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(term))
            {
                ManageSuppliersGrid.ItemsSource = _suppliers;
                return;
            }

            var results = _suppliers
                .Where(s =>
                    (!string.IsNullOrEmpty(s.CodeFournisseur) && s.CodeFournisseur.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(s.Designation) && s.Designation.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            ManageSuppliersGrid.ItemsSource = results;
        }

        private void OpenEditDialogButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = ManageSuppliersGrid.SelectedItem as Supplier;
            if (selected == null)
            {
                MessageBox.Show("Sélectionnez un fournisseur à modifier.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var editWindow = new SupplierEditWindow(selected)
                {
                    Owner = Application.Current.MainWindow
                };

                bool? result = editWindow.ShowDialog();

                if (result == true)
                {
                    _db.SaveChanges();
                    ManageSuppliersGrid.Items.Refresh();
                    SuppliersGrid.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la modification : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrepareNewSupplier()
        {
            CodeFournisseurTextBox.Text = GenerateSupplierCode();
            DesignationTextBox.Text = "";
            ActiviteTextBox.Text = "";
            AdresseTextBox.Text = "";
            RcTextBox.Text = "";
            MatriculeTextBox.Text = "";
            TypeIdComboBox.SelectedIndex = 0;
            NumeroIdTextBox.Text = "";
            _selectedSupplier = null;
        }

        private string GenerateSupplierCode()
        {
            try
            {
                string? lastCode = _db.Suppliers
                    .OrderByDescending(s => s.Id)
                    .Select(s => s.CodeFournisseur)
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(lastCode))
                    return "F001";

                if (!lastCode.StartsWith("F") || lastCode.Length < 2)
                    return "F001";

                if (!int.TryParse(lastCode.Substring(1), out int lastNumber))
                    return "F001";

                int newNumber = lastNumber + 1;
                return "F" + newNumber.ToString("D3");
            }
            catch
            {
                return "F001";
            }
        }

        private void NewSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            PrepareNewSupplier();
        }

        private void SaveSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DesignationTextBox.Text))
            {
                MessageBox.Show("Désignation obligatoire.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                DesignationTextBox.Focus();
                return;
            }

            try
            {
                var typeId = (TypeIdComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString()
                             ?? TypeIdComboBox.Text;

                var supplier = new Supplier
                {
                    CodeFournisseur = CodeFournisseurTextBox.Text,
                    Designation = DesignationTextBox.Text.Trim(),
                    Activite = ActiviteTextBox.Text?.Trim(),
                    Adresse = AdresseTextBox.Text?.Trim(),
                    NumeroRC = RcTextBox.Text?.Trim(),
                    MatriculeFiscal = MatriculeTextBox.Text?.Trim(),
                    TypeIdentification = typeId,
                    NumeroIdentification = NumeroIdTextBox.Text?.Trim(),
                    TotalAchats = null,
                    Dette = null,
                    EstActif = true,
                    DateCreation = DateTime.Now
                };

                _db.Suppliers.Add(supplier);
                _db.SaveChanges();

                _suppliers.Add(supplier);

                MessageBox.Show("Fournisseur enregistré avec succès.",
                    "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                PrepareNewSupplier();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'enregistrement : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SuppliersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedSupplier = SuppliersGrid.SelectedItem as Supplier;
            if (_selectedSupplier == null)
                return;

            CodeFournisseurTextBox.Text = _selectedSupplier.CodeFournisseur;
            DesignationTextBox.Text = _selectedSupplier.Designation;
            ActiviteTextBox.Text = _selectedSupplier.Activite;
            AdresseTextBox.Text = _selectedSupplier.Adresse;
            RcTextBox.Text = _selectedSupplier.NumeroRC;
            MatriculeTextBox.Text = _selectedSupplier.MatriculeFiscal;
            TypeIdComboBox.Text = _selectedSupplier.TypeIdentification;
            NumeroIdTextBox.Text = _selectedSupplier.NumeroIdentification;
        }

        private void OpenCardButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = ManageSuppliersGrid.SelectedItem as Supplier;
            if (selected == null)
            {
                MessageBox.Show("Sélectionnez un fournisseur pour afficher sa carte.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var cardWindow = new SupplierCardWindow(selected)
                {
                    Owner = Application.Current.MainWindow
                };
                cardWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'ouverture de la carte : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
