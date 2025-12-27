using System;
using System.Windows;

namespace ProManSystem.Views
{
    public partial class QuantityEditDialog : Window
    {
        public decimal EnteredQuantity { get; private set; }
        public decimal EnteredPrice { get; private set; }

        public QuantityEditDialog(string productName, decimal currentQuantity, decimal currentPrice)
        {
            InitializeComponent();

            ProductNameTextBlock.Text = productName;
            QuantityTextBox.Text = currentQuantity.ToString("0.##");
            PriceTextBox.Text = currentPrice.ToString("0.##");

            QuantityTextBox.Focus();
            QuantityTextBox.SelectAll();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
       
            if (!decimal.TryParse(QuantityTextBox.Text.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal quantity))
            {
                MessageBox.Show("Veuillez saisir une quantité valide.", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantityTextBox.Focus();
                return;
            }

            if (quantity <= 0)
            {
                MessageBox.Show("La quantité doit être supérieure à zéro.", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantityTextBox.Focus();
                return;
            }

          
            if (!decimal.TryParse(PriceTextBox.Text.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal price))
            {
                MessageBox.Show("Veuillez saisir un prix valide.", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PriceTextBox.Focus();
                return;
            }

            if (price <= 0)
            {
                MessageBox.Show("Le prix doit être supérieur à zéro.", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PriceTextBox.Focus();
                return;
            }

            EnteredQuantity = quantity;
            EnteredPrice = price;

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
