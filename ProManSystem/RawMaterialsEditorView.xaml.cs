using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProManSystem.Views
{
    public partial class RawMaterialsEditorView : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();

        private ObservableCollection<RawMaterial> _materials = new();
        private ObservableCollection<Unit> _units = new();

        public RawMaterialsEditorView()
        {
            InitializeComponent();
            LoadUnits();
            LoadMaterials();
            PrepareNewMaterial();

            RawMaterialsGrid.ItemsSource = _materials;
            ManageRawMaterialsGrid.ItemsSource = _materials;
        }

        private void LoadUnits()
        {
            try
            {
                if (!_db.Units.Any())
                {
                    var predefined = new[]
                    {
                        "Mètre", "Kilogramme", "Pièce", "Litre", "Tonne", "Carton",
                        "Gramme", "Centimètre", "Millimètre", "Unité", "Boîte", "Sachet"
                    };

                    foreach (var name in predefined)
                    {
                        _db.Units.Add(new Unit { Nom = name, EstPredefined = true });
                    }

                    _db.SaveChanges();
                }

                _units = new ObservableCollection<Unit>(_db.Units.OrderBy(u => u.Nom).ToList());
                UnitComboBox.ItemsSource = _units;
                UnitComboBox.DisplayMemberPath = "Nom";

                if (_units.Count > 0)
                    UnitComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des unités : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMaterials()
        {
            try
            {
                _materials = new ObservableCollection<RawMaterial>(
                    _db.RawMaterials
                       .OrderBy(m => m.CodeMatiere)
                       .ToList()
                );

                RawMaterialsGrid.ItemsSource = _materials;
                ManageRawMaterialsGrid.ItemsSource = _materials;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des matières : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrepareNewMaterial()
        {
            CodeMatiereTextBox.Text = GenerateCode();
            DesignationTextBox.Text = "";
            StockInitialTextBox.Text = "0";
            StockMinTextBox.Text = "0";
            PMAPATextBox.Text = "0.00";

            if (UnitComboBox.Items.Count > 0)
                UnitComboBox.SelectedIndex = 0;

            DesignationTextBox.Focus();
        }

        private string GenerateCode()
        {
            try
            {
                string? lastCode = _db.RawMaterials
                    .OrderByDescending(m => m.Id)
                    .Select(m => m.CodeMatiere)
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(lastCode))
                    return "M001";

                if (!lastCode.StartsWith("M") || lastCode.Length < 2)
                    return "M001";

                if (!int.TryParse(lastCode.Substring(1), out int num))
                    return "M001";

                return "M" + (num + 1).ToString("D3");
            }
            catch
            {
                return "M001";
            }
        }

        private void NewMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            PrepareNewMaterial();
        }

        private void SaveMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DesignationTextBox.Text))
            {
                MessageBox.Show("La désignation est obligatoire.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                DesignationTextBox.Focus();
                return;
            }

            if (UnitComboBox.SelectedItem is not Unit selectedUnit)
            {
                MessageBox.Show("Sélectionnez une unité.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                UnitComboBox.Focus();
                return;
            }

            decimal stockMin = 0;
            if (!string.IsNullOrWhiteSpace(StockMinTextBox.Text))
            {
                var txt = StockMinTextBox.Text.Replace(',', '.');
                if (!decimal.TryParse(txt, NumberStyles.Any, CultureInfo.InvariantCulture, out stockMin))
                {
                    MessageBox.Show("Stock min invalide.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    StockMinTextBox.Focus();
                    return;
                }
            }

            try
            {
                var material = new RawMaterial
                {
                    CodeMatiere = CodeMatiereTextBox.Text,
                    Designation = DesignationTextBox.Text.Trim(),
                    UnitId = selectedUnit.Id,
                    StockInitial = 0m,
                    StockActuel = 0m,
                    StockMin = stockMin,
                    PMAPA = 0m,
                    DateCreation = DateTime.Now
                };

                _db.RawMaterials.Add(material);
                _db.SaveChanges();

                var savedMaterial = _db.RawMaterials
                    .Where(m => m.Id == material.Id)
                    .Select(m => new RawMaterial
                    {
                        Id = m.Id,
                        CodeMatiere = m.CodeMatiere,
                        Designation = m.Designation,
                        UnitId = m.UnitId,
                        Unit = m.Unit,
                        StockInitial = m.StockInitial,
                        StockActuel = m.StockActuel,
                        StockMin = m.StockMin,
                        PMAPA = m.PMAPA,
                        DateCreation = m.DateCreation
                    })
                    .FirstOrDefault();

                if (savedMaterial != null)
                {
                    _materials.Add(savedMaterial);
                    RawMaterialsGrid.Items.Refresh();
                    ManageRawMaterialsGrid.Items.Refresh();
                }

                MessageBox.Show("Matière enregistrée avec succès.\n\nAjoutez du stock via 'Factures achat'.",
                    "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                PrepareNewMaterial();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'enregistrement : " + ex.Message + "\n\n" + ex.InnerException?.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not RawMaterial material)
                return;

            var result = MessageBox.Show(
                $"Voulez-vous vraiment supprimer la matière :\n\n{material.CodeMatiere} - {material.Designation}\n\nCette action est irréversible.",
                "Confirmation de suppression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                var hasRecipes = _db.ProductRecipes.Any(pr => pr.RawMaterialId == material.Id);
                if (hasRecipes)
                {
                    MessageBox.Show(
                        "Cette matière est utilisée dans des recettes de produits.\n\nSupprimez d'abord les recettes associées.",
                        "Suppression impossible",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var hasPurchases = _db.PurchaseInvoiceLines.Any(l => l.RawMaterialId == material.Id);
                if (hasPurchases)
                {
                    var confirm = MessageBox.Show(
                        "Cette matière a des historiques d'achats.\n\nVoulez-vous vraiment la supprimer ?",
                        "Confirmation",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (confirm != MessageBoxResult.Yes)
                        return;
                }

                var dbMaterial = _db.RawMaterials.FirstOrDefault(m => m.Id == material.Id);
                if (dbMaterial != null)
                {
                    _db.RawMaterials.Remove(dbMaterial);
                    _db.SaveChanges();

                    _materials.Remove(material);
                    RawMaterialsGrid.Items.Refresh();
                    ManageRawMaterialsGrid.Items.Refresh();

                    MessageBox.Show("Matière supprimée avec succès.",
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la suppression : " + ex.Message + "\n\n" + ex.InnerException?.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string term = (SearchTextBox.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(term))
            {
                RawMaterialsGrid.ItemsSource = _materials;
                return;
            }

            var results = _materials
                .Where(m =>
                    (!string.IsNullOrEmpty(m.CodeMatiere) &&
                     m.CodeMatiere.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(m.Designation) &&
                     m.Designation.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            RawMaterialsGrid.ItemsSource = results;
        }

        private void ManageSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string term = (ManageSearchTextBox.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(term))
            {
                ManageRawMaterialsGrid.ItemsSource = _materials;
                return;
            }

            var results = _materials
                .Where(m =>
                    (!string.IsNullOrEmpty(m.CodeMatiere) &&
                     m.CodeMatiere.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(m.Designation) &&
                     m.Designation.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            ManageRawMaterialsGrid.ItemsSource = results;
        }

        private void AddUnitButton_Click(object sender, RoutedEventArgs e)
        {
            string nom = Microsoft.VisualBasic.Interaction.InputBox(
                "أدخل اسم الوحدة الجديدة :", "Nouvelle unité", "");

            if (string.IsNullOrWhiteSpace(nom))
                return;

            nom = nom.Trim();

            if (_units.Any(u => u.Nom.Equals(nom, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Cette unité existe déjà.",
                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var unit = new Unit
                {
                    Nom = nom,
                    EstPredefined = false
                };

                _db.Units.Add(unit);
                _db.SaveChanges();

                _units.Add(unit);

                var sortedUnits = _units.OrderBy(u => u.Nom).ToList();
                _units.Clear();
                foreach (var u in sortedUnits)
                    _units.Add(u);

                UnitComboBox.ItemsSource = _units;
                UnitComboBox.SelectedItem = unit;

                MessageBox.Show("Unité ajoutée avec succès.",
                    "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'ajout de l'unité : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
