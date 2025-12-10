using ProManSystem.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ProManSystem.Views
{
    public partial class CustomerEditWindow : Window
    {
        private Customer _customer;

        public CustomerEditWindow(Customer customer)
        {
            InitializeComponent();
            _customer = customer;
            LoadCustomerData();
        }

        private void LoadCustomerData()
        {
            CodeClientTextBox.Text = _customer.CodeClient;
            NomTextBox.Text = _customer.NomComplet;
            ActiviteTextBox.Text = _customer.Activite;
            AdresseTextBox.Text = _customer.Adresse;
            RcTextBox.Text = _customer.NumeroRC;
            MatriculeTextBox.Text = _customer.MatriculeFiscal;
            NumeroIdTextBox.Text = _customer.NumeroIdentification;

            
            if (_customer.TypeIdentification == "Article")
                TypeIdComboBox.SelectedIndex = 0;
            else if (_customer.TypeIdentification == "BP")
                TypeIdComboBox.SelectedIndex = 1;
            else
                TypeIdComboBox.Text = _customer.TypeIdentification;

           
            CAHTTextBox.Text = _customer.CA_HT?.ToString("F2") ?? "0.00";
            TVATextBox.Text = _customer.TauxTVA?.ToString("F2") ?? "0.00";
            CATTCTextBox.Text = _customer.CA_TTC?.ToString("F2") ?? "0.00";

            
            bool isRadie = _customer.EstRadie;
            RadieCheckBox.IsChecked = isRadie;
            RadieCheckBox2.IsChecked = isRadie;

            if (_customer.DateRadiation.HasValue)
                DateRadiationPicker.SelectedDate = _customer.DateRadiation.Value;

            UpdateRadieUI(isRadie);

            
            HeaderSubtitle.Text = $"Code: {_customer.CodeClient} - {_customer.NomComplet}";
        }

        private void RadieCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool isChecked = RadieCheckBox.IsChecked == true || RadieCheckBox2.IsChecked == true;

           
            RadieCheckBox.IsChecked = isChecked;
            RadieCheckBox2.IsChecked = isChecked;

            UpdateRadieUI(isChecked);

          
            if (isChecked && !DateRadiationPicker.SelectedDate.HasValue)
                DateRadiationPicker.SelectedDate = DateTime.Now;
        }

        private void UpdateRadieUI(bool isRadie)
        {
            if (isRadie)
            {
                RadieBorder.Visibility = Visibility.Visible;
                NonRadieBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                RadieBorder.Visibility = Visibility.Collapsed;
                NonRadieBorder.Visibility = Visibility.Visible;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
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
                _customer.NomComplet = NomTextBox.Text.Trim();
                _customer.Activite = ActiviteTextBox.Text?.Trim();
                _customer.Adresse = AdresseTextBox.Text?.Trim();
                _customer.NumeroRC = RcTextBox.Text?.Trim();
                _customer.MatriculeFiscal = MatriculeTextBox.Text?.Trim();
                _customer.NumeroIdentification = NumeroIdTextBox.Text?.Trim();

                var typeId = (TypeIdComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString()
                             ?? TypeIdComboBox.Text;
                _customer.TypeIdentification = typeId;

                _customer.EstRadie = RadieCheckBox.IsChecked == true;
                _customer.DateRadiation = DateRadiationPicker.SelectedDate;

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la sauvegarde : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
