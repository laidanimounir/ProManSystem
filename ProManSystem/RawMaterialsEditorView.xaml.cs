using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Collections.ObjectModel;
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
                        "Mètre", "Kilogramme", "Pièce", "Litre", "Tonne", "Carton"
                    };

                    foreach (var name in predefined)
                        _db.Units.Add(new Unit { Nom = name, EstPredefined = true });

                    _db.SaveChanges();
                }

                _units = new ObservableCollection<Unit>(_db.Units.OrderBy(u => u.Nom).ToList());
                UnitComboBox.ItemsSource = _units;
                UnitComboBox.DisplayMemberPath = "Nom";
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
                return;
            }

            if (!decimal.TryParse(StockInitialTextBox.Text.Replace('.', ','), out decimal stockIni))
                stockIni = 0;

            if (!decimal.TryParse(StockMinTextBox.Text.Replace('.', ','), out decimal stockMin))
                stockMin = 0;

            try
            {
                var material = new RawMaterial
                {
                    CodeMatiere = CodeMatiereTextBox.Text,
                    Designation = DesignationTextBox.Text.Trim(),
                    UnitId = selectedUnit.Id,
                    StockInitial = stockIni,
                    StockActuel = stockIni,
                    StockMin = stockMin,
                    PMAPA = 0m,
                    DateCreation = DateTime.Now
                };

                _db.RawMaterials.Add(material);
                _db.SaveChanges();

                _materials.Add(material);
                RawMaterialsGrid.Items.Refresh();
                ManageRawMaterialsGrid.Items.Refresh();

                MessageBox.Show("Matière enregistrée avec succès.",
                    "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                PrepareNewMaterial();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'enregistrement : " + ex.Message,
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

        private void OpenEditDialogButton_Click(object sender, RoutedEventArgs e)
        {
           
            MessageBox.Show("التعديل التفصيلي للمادة سنضيفه بعد الانتهاء من فواتير الشراء.",
                "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddUnitButton_Click(object sender, RoutedEventArgs e)
        {
            string nom = Microsoft.VisualBasic.Interaction.InputBox(
                "أدخل اسم الوحدة الجديدة :", "Nouvelle unité", "");

            if (string.IsNullOrWhiteSpace(nom))
                return;

            try
            {
                var unit = new Unit
                {
                    Nom = nom.Trim(),
                    EstPredefined = false
                };

                _db.Units.Add(unit);
                _db.SaveChanges();

                _units.Add(unit);
                UnitComboBox.ItemsSource = _units;
                UnitComboBox.SelectedItem = unit;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'ajout de l'unité : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
