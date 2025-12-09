using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using ProManSystem.Data;
using ProManSystem.Models;

namespace ProManSystem
{
    public partial class MainWindow : Window
    {
        private readonly AppDbContext _db = new AppDbContext();
        private ObservableCollection<Customer> _customers = new ObservableCollection<Customer>();
        private Customer? _selectedCustomer;   

        public MainWindow()
        {
            InitializeComponent();
            LoadCustomers();
            PrepareNewCustomer();
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
                MessageBox.Show("Erreur lors du chargement des clients : " + ex.Message);
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
            TypeIdComboBox.SelectedIndex = -1;
            NumeroIdTextBox.Text = "";
            CAHTTextBox.Text = "";
            TVATextBox.Text = "";
            CATTCTextBox.Text = "";
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

        private decimal? ParseDecimal(string text)
        {
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                return value;
            if (decimal.TryParse(text, out value))
                return value;
            return null;
        }

        private void NewCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            PrepareNewCustomer();
        }

        private void SaveCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NomTextBox.Text))
            {
                MessageBox.Show("Nom / Raison sociale obligatoire.");
                return;
            }

            var caHt = ParseDecimal(CAHTTextBox.Text);
            var tva = ParseDecimal(TVATextBox.Text);
            decimal? caTtc = null;

            if (caHt.HasValue && tva.HasValue)
            {
                caTtc = caHt.Value + (caHt.Value * tva.Value / 100m);
            }

            var typeId = (TypeIdComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString()
                         ?? TypeIdComboBox.Text;

            var customer = new Customer
            {
                CodeClient = CodeClientTextBox.Text,
                NomComplet = NomTextBox.Text,
                Activite = ActiviteTextBox.Text,
                Adresse = AdresseTextBox.Text,
                NumeroRC = RcTextBox.Text,
                MatriculeFiscal = MatriculeTextBox.Text,
                TypeIdentification = typeId,
                NumeroIdentification = NumeroIdTextBox.Text,
                CA_HT = caHt,
                TauxTVA = tva,
                CA_TTC = caTtc
            };

            _db.Customers.Add(customer);
            _db.SaveChanges();

            _customers.Add(customer);

            PrepareNewCustomer();
        }

       
        private void CustomersGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selectedCustomer = CustomersGrid.SelectedItem as Customer;
            if (_selectedCustomer == null) return;

            CodeClientTextBox.Text = _selectedCustomer.CodeClient;
            NomTextBox.Text = _selectedCustomer.NomComplet;
            ActiviteTextBox.Text = _selectedCustomer.Activite;
            AdresseTextBox.Text = _selectedCustomer.Adresse;
            RcTextBox.Text = _selectedCustomer.NumeroRC;
            MatriculeTextBox.Text = _selectedCustomer.MatriculeFiscal;
            TypeIdComboBox.Text = _selectedCustomer.TypeIdentification;
            NumeroIdTextBox.Text = _selectedCustomer.NumeroIdentification;
            CAHTTextBox.Text = _selectedCustomer.CA_HT?.ToString();
            TVATextBox.Text = _selectedCustomer.TauxTVA?.ToString();
            CATTCTextBox.Text = _selectedCustomer.CA_TTC?.ToString();
        }

        
        private void UpdateCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                MessageBox.Show("Sélectionnez un client à modifier.");
                return;
            }

            if (string.IsNullOrWhiteSpace(NomTextBox.Text))
            {
                MessageBox.Show("Nom / Raison sociale obligatoire.");
                return;
            }

            var caHt = ParseDecimal(CAHTTextBox.Text);
            var tva = ParseDecimal(TVATextBox.Text);
            decimal? caTtc = null;

            if (caHt.HasValue && tva.HasValue)
                caTtc = caHt.Value + (caHt.Value * tva.Value / 100m);

            var typeId = (TypeIdComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString()
                         ?? TypeIdComboBox.Text;

            _selectedCustomer.NomComplet = NomTextBox.Text;
            _selectedCustomer.Activite = ActiviteTextBox.Text;
            _selectedCustomer.Adresse = AdresseTextBox.Text;
            _selectedCustomer.NumeroRC = RcTextBox.Text;
            _selectedCustomer.MatriculeFiscal = MatriculeTextBox.Text;
            _selectedCustomer.TypeIdentification = typeId;
            _selectedCustomer.NumeroIdentification = NumeroIdTextBox.Text;
            _selectedCustomer.CA_HT = caHt;
            _selectedCustomer.TauxTVA = tva;
            _selectedCustomer.CA_TTC = caTtc;

            _db.SaveChanges();              
            CustomersGrid.Items.Refresh();   
        }

        
        private void DeleteCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                MessageBox.Show("Sélectionnez un client à supprimer.");
                return;
            }

            var result = MessageBox.Show(
                $"Supprimer le client {_selectedCustomer.CodeClient} ?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            _db.Customers.Remove(_selectedCustomer);
            _db.SaveChanges();              
            _customers.Remove(_selectedCustomer);
            _selectedCustomer = null;

            PrepareNewCustomer();
        }
    }
}
