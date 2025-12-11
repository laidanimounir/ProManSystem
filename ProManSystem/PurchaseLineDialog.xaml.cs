using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

            QuantiteTextBox.TextChanged += AnyField_TextChanged;
            PrixTextBox.TextChanged += AnyField_TextChanged;
        }

        private void LoadMaterials()
        {
            try
            {
                var mats = _db.RawMaterials
                    .Include(m=>m.Unit)
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
            var qTxt = (QuantiteTextBox.Text ?? "0").Replace('.', ',');
            var pTxt = (PrixTextBox.Text ?? "0").Replace('.', ',');

            if (!decimal.TryParse(qTxt, out var q))
                q = 0;
            if (!decimal.TryParse(pTxt, out var p))
                p = 0;

            var total = q * p;
            TotalTextBox.Text = total.ToString("0.00");
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialComboBox.SelectedItem is not RawMaterial mat)
            {
                MessageBox.Show("اختر مادة أولاً.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var qTxt = (QuantiteTextBox.Text ?? "").Replace('.', ',');
            var pTxt = (PrixTextBox.Text ?? "").Replace('.', ',');

            if (!decimal.TryParse(qTxt, out var q) || q <= 0)
            {
                MessageBox.Show("الكمية غير صحيحة.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(pTxt, out var p) || p < 0)
            {
                MessageBox.Show("السعر غير صحيح.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CreatedLine = new PurchaseInvoiceLine
            {
                RawMaterialId = mat.Id,
                RawMaterial = mat,
                Quantite = q,
                PrixUnitaire = p,
                MontantLigne = q * p
            };

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
