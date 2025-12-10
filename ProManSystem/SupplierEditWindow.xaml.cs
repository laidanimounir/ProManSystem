using ProManSystem.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ProManSystem.Views
{
    public partial class SupplierEditWindow : Window
    {
        private Supplier _supplier;

        public SupplierEditWindow(Supplier supplier)
        {
            InitializeComponent();
            _supplier = supplier;
            LoadSupplierData();
        }

        private void LoadSupplierData()
        {
            CodeFournisseurTextBox.Text = _supplier.CodeFournisseur;
            DesignationTextBox.Text = _supplier.Designation;
            ActiviteTextBox.Text = _supplier.Activite;
            AdresseTextBox.Text = _supplier.Adresse;
            RcTextBox.Text = _supplier.NumeroRC;
            MatriculeTextBox.Text = _supplier.MatriculeFiscal;
            NumeroIdTextBox.Text = _supplier.NumeroIdentification;

            
            if (_supplier.TypeIdentification == "Article")
                TypeIdComboBox.SelectedIndex = 0;
            else if (_supplier.TypeIdentification == "BP")
                TypeIdComboBox.SelectedIndex = 1;
            else
                TypeIdComboBox.Text = _supplier.TypeIdentification;

            
            TotalAchatsTextBox.Text = _supplier.TotalAchats?.ToString("F2") ?? "0.00";
            DetteTextBox.Text = _supplier.Dette?.ToString("F2") ?? "0.00";

            
            bool isActif = _supplier.EstActif;
            ActifCheckBox.IsChecked = isActif;
            ActifCheckBox2.IsChecked = !isActif;

            UpdateActifUI(isActif);

           
            HeaderSubtitle.Text = $"Code: {_supplier.CodeFournisseur} - {_supplier.Designation}";
        }

        private void ActifCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool isActif = ActifCheckBox.IsChecked == true;

            
            ActifCheckBox.IsChecked = isActif;
            ActifCheckBox2.IsChecked = !isActif;

            UpdateActifUI(isActif);
        }

        private void UpdateActifUI(bool isActif)
        {
            if (isActif)
            {
                ActifBorder.Visibility = Visibility.Visible;
                InactifBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                ActifBorder.Visibility = Visibility.Collapsed;
                InactifBorder.Visibility = Visibility.Visible;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
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
                _supplier.Designation = DesignationTextBox.Text.Trim();
                _supplier.Activite = ActiviteTextBox.Text?.Trim();
                _supplier.Adresse = AdresseTextBox.Text?.Trim();
                _supplier.NumeroRC = RcTextBox.Text?.Trim();
                _supplier.MatriculeFiscal = MatriculeTextBox.Text?.Trim();
                _supplier.NumeroIdentification = NumeroIdTextBox.Text?.Trim();

                var typeId = (TypeIdComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString()
                             ?? TypeIdComboBox.Text;
                _supplier.TypeIdentification = typeId;

                _supplier.EstActif = ActifCheckBox.IsChecked == true;

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
