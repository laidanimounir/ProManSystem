using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

namespace ProManSystem.Views
{
    public partial class PurchaseLineDialog : Window
    {
        private readonly AppDbContext _db = new AppDbContext();

        public PurchaseInvoiceLine? CreatedLine { get; private set; }

        public PurchaseLineDialog()
        {
            InitializeComponent();
            LoadMaterials();

            QuantiteTextBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
            PrixTextBox.PreviewTextInput += NumericTextBox_PreviewTextInput;

            QuantiteTextBox.TextChanged += AnyField_TextChanged;
            PrixTextBox.TextChanged += AnyField_TextChanged;
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
                return;

            string fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            if (string.IsNullOrWhiteSpace(e.Text))
            {
                e.Handled = false;
                return;
            }

            if (e.Text == "." || e.Text == ",")
            {
                if (textBox.Text.Contains(".") || textBox.Text.Contains(","))
                {
                    e.Handled = true;
                    return;
                }
                e.Handled = false;
                return;
            }

            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
                return;
            }

            e.Handled = false;
        }

        private void LoadMaterials()
        {
            try
            {
                var mats = _db.RawMaterials
                    .Include(m => m.Unit)
                    .OrderBy(m => m.CodeMatiere)
                    .ToList();

                MaterialComboBox.ItemsSource = mats;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des matières : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AnyField_TextChanged(object sender, TextChangedEventArgs e)
        {
            var qTxt = (QuantiteTextBox.Text ?? "0").Replace(',', '.');
            var pTxt = (PrixTextBox.Text ?? "0").Replace(',', '.');

            decimal q = 0;
            decimal p = 0;

            if (!decimal.TryParse(qTxt, NumberStyles.Any, CultureInfo.InvariantCulture, out q))
                q = 0;

            if (!decimal.TryParse(pTxt, NumberStyles.Any, CultureInfo.InvariantCulture, out p))
                p = 0;

            var total = q * p;
            TotalTextBox.Text = total.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialComboBox.SelectedItem is not RawMaterial mat)
            {
                MessageBox.Show("اختر مادة أولاً.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                MaterialComboBox.Focus();
                return;
            }

            var qTxt = (QuantiteTextBox.Text ?? "").Replace(',', '.');
            var pTxt = (PrixTextBox.Text ?? "").Replace(',', '.');

            decimal q = 0;
            decimal p = 0;

            if (!decimal.TryParse(qTxt, NumberStyles.Any, CultureInfo.InvariantCulture, out q) || q <= 0)
            {
                MessageBox.Show("الكمية غير صحيحة. يجب أن تكون أكبر من صفر.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantiteTextBox.Focus();
                QuantiteTextBox.SelectAll();
                return;
            }

            if (!decimal.TryParse(pTxt, NumberStyles.Any, CultureInfo.InvariantCulture, out p) || p < 0)
            {
                MessageBox.Show("السعر غير صحيح.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PrixTextBox.Focus();
                PrixTextBox.SelectAll();
                return;
            }

            CreatedLine = new PurchaseInvoiceLine
            {
                RawMaterialId = mat.Id,
                RawMaterial = mat,
                Quantite = q,
                PrixUnitaire = p,
                MontantLigne = Math.Round(q * p, 2)
            };

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }


        private void MaterialComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // حالياً لا نحتاج منطق خاص عند تغيير المادة
           
        }
    }
}
