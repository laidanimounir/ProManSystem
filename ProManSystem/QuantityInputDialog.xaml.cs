using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ProManSystem.Views
{
    public partial class QuantityInputDialog : Window
    {
        public decimal EnteredQuantity { get; private set; }
        private readonly decimal _maxQuantity;

        public QuantityInputDialog(decimal maxQuantity)
        {
            InitializeComponent();
            _maxQuantity = maxQuantity;

            MaxLabel.Text = $"(أقصى كمية متاحة: {_maxQuantity})";

            QuantityTextBox.Text = "1";
            QuantityTextBox.SelectAll();
            QuantityTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var txt = (QuantityTextBox.Text ?? string.Empty).Replace('.', ',');

            if (!decimal.TryParse(txt, out var q) || q <= 0)
            {
                MessageBox.Show("الكمية غير صحيحة.", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EnteredQuantity = q;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // السماح فقط بالأرقام والفاصلة/النقطة
        private void QuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9.,]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
