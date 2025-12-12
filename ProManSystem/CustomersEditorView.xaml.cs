using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProManSystem.Views
{
    public partial class CustomersEditorView : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();
        private ObservableCollection<Customer> _customers = new ObservableCollection<Customer>();
        private Customer? _selectedCustomer;

        public CustomersEditorView()
        {
            InitializeComponent();
            LoadCustomers();
            PrepareNewCustomer();
            ManageCustomersGrid.ItemsSource = _customers;
        }

        private void LoadCustomers()
        {
            try
            {
                var list = _db.Customers.ToList();
                _customers = new ObservableCollection<Customer>(list);
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
            string term = SearchTextBox.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(term))
            {
                CustomersGrid.ItemsSource = _customers;
                return;
            }

            var results = _customers
                .Where(c =>
                    (!string.IsNullOrEmpty(c.CodeClient) && c.CodeClient.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(c.NomComplet) && c.NomComplet.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            CustomersGrid.ItemsSource = results;
        }

        private void ManageSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string term = ManageSearchTextBox.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(term))
            {
                ManageCustomersGrid.ItemsSource = _customers;
                return;
            }

            var results = _customers
                .Where(c =>
                    (!string.IsNullOrEmpty(c.CodeClient) && c.CodeClient.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(c.NomComplet) && c.NomComplet.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            ManageCustomersGrid.ItemsSource = results;
        }

        private void OpenEditDialogButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = ManageCustomersGrid.SelectedItem as Customer;
            if (selected == null)
            {
                MessageBox.Show("Sélectionnez un client à modifier.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var editWindow = new CustomerEditWindow(selected)
                {
                    Owner = Application.Current.MainWindow
                };

                bool? result = editWindow.ShowDialog();

                if (result == true)
                {
                    _db.SaveChanges();
                    ManageCustomersGrid.Items.Refresh();
                    CustomersGrid.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la modification : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrepareNewCustomer()
        {
            CodeClientTextBox.Text = GenerateCustomerCode();
            NomTextBox.Text = "";
            ActiviteTextBox.Text = "";
            AdresseTextBox.Text = "";
            RcTextBox.Text = "";
            MatriculeTextBox.Text = "";
            TypeIdComboBox.SelectedIndex = 0;
            NumeroIdTextBox.Text = "";
            CategorieComboBox.Text = "";
            DateCreationTextBox.Text = DateTime.Now.ToString("dd/MM/yyyy");
            _selectedCustomer = null;
        }



        private string GenerateCustomerCode()
        {
            try
            {
                string? lastCode = _db.Customers
                    .OrderByDescending(c => c.Id)
                    .Select(c => c.CodeClient)
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(lastCode))
                    return "C001";

                if (!lastCode.StartsWith("C") || lastCode.Length < 2)
                    return "C001";

                if (!int.TryParse(lastCode.Substring(1), out int lastNumber))
                    return "C001";

                int newNumber = lastNumber + 1;
                return "C" + newNumber.ToString("D3");
            }
            catch
            {
                return "C001";
            }
        }

        private void NewCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            PrepareNewCustomer();
        }

        private void SaveCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NomTextBox.Text))
            {
                MessageBox.Show("Nom / Raison sociale obligatoire.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                NomTextBox.Focus();
                return;
            }

            try
            {
                var typeId = (TypeIdComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString()
                             ?? TypeIdComboBox.Text;

                var customer = new Customer
                {
                    CodeClient = CodeClientTextBox.Text,
                    NomComplet = NomTextBox.Text.Trim(),
                    Activite = ActiviteTextBox.Text?.Trim(),
                    Adresse = AdresseTextBox.Text?.Trim(),
                    NumeroRC = RcTextBox.Text?.Trim(),
                    MatriculeFiscal = MatriculeTextBox.Text?.Trim(),
                    TypeIdentification = typeId,
                    NumeroIdentification = NumeroIdTextBox.Text?.Trim(),
                    Categorie = CategorieComboBox.Text?.Trim(),
                    PhotoPath = null, 
                    DateCreation = DateTime.Now,
                    CA_HT = null,
                    TauxTVA = null,
                    CA_TTC = null
                };



                _db.Customers.Add(customer);
                _db.SaveChanges();

                _customers.Add(customer);

                MessageBox.Show("Client enregistré avec succès.",
                    "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                PrepareNewCustomer();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'enregistrement : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CustomersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedCustomer = CustomersGrid.SelectedItem as Customer;
            if (_selectedCustomer == null)
                return;

            CodeClientTextBox.Text = _selectedCustomer.CodeClient;
            NomTextBox.Text = _selectedCustomer.NomComplet;
            ActiviteTextBox.Text = _selectedCustomer.Activite;
            AdresseTextBox.Text = _selectedCustomer.Adresse;
            RcTextBox.Text = _selectedCustomer.NumeroRC;
            MatriculeTextBox.Text = _selectedCustomer.MatriculeFiscal;
            TypeIdComboBox.Text = _selectedCustomer.TypeIdentification;
            NumeroIdTextBox.Text = _selectedCustomer.NumeroIdentification;
            CategorieComboBox.Text = _selectedCustomer.Categorie;
            DateCreationTextBox.Text = _selectedCustomer.DateCreation.ToString("dd/MM/yyyy");


        }

        private void OpenCardButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = ManageCustomersGrid.SelectedItem as Customer;
            if (selected == null)
            {
                MessageBox.Show("Sélectionnez un client pour afficher sa carte.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var cardWindow = new CustomerCardWindow(selected)
            {
                Owner = Application.Current.MainWindow
            };
            cardWindow.ShowDialog();
        }

    }

}
