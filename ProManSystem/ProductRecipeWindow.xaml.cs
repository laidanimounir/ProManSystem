using ProManSystem.Data;
using ProManSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ProManSystem.Views
{
    public partial class ProductRecipeWindow : Window
    {
        private readonly AppDbContext _db = new AppDbContext();
        private readonly int _productId;
        private List<RawMaterial> _allRawMaterials = new List<RawMaterial>();
        private List<ProductRecipe> _recipeRows = new List<ProductRecipe>();

        public ProductRecipeWindow(int productId)
        {
            InitializeComponent();
            _productId = productId;

            LoadData();
        }

        private void LoadData()
        {
            
            var product = _db.Products.FirstOrDefault(p => p.Id == _productId);
            if (product == null)
            {
                MessageBox.Show("لم يتم العثور على المنتج.", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            ProductTitleTextBlock.Text = $"وصفة المنتج: {product.Nom}";

           
            _allRawMaterials = _db.RawMaterials
                .OrderBy(r => r.Designation)
                .ToList();

           
            _recipeRows = _db.ProductRecipes
                .Where(pr => pr.ProductId == _productId)
                .OrderBy(pr => pr.Id)
                .ToList();

         
            var comboColumn = RecipeGrid.Columns[0] as System.Windows.Controls.DataGridComboBoxColumn;
            if (comboColumn != null)
            {
                comboColumn.ItemsSource = _allRawMaterials;
                comboColumn.DisplayMemberPath = "Designation";
                comboColumn.SelectedValuePath = "Id";
            }

            RecipeGrid.ItemsSource = _recipeRows;
        }

        private void AddRowButton_Click(object sender, RoutedEventArgs e)
        {
            var newRow = new ProductRecipe
            {
                ProductId = _productId,
                QuantiteNecessaire = 0
            };
            _recipeRows.Add(newRow);
            RefreshGrid();
        }

        private void DeleteRowButton_Click(object sender, RoutedEventArgs e)
        {
            var row = (sender as FrameworkElement)?.DataContext as ProductRecipe;
            if (row == null) return;

            if (MessageBox.Show("هل تريد حذف هذا السطر من الوصفة؟",
                    "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.No)
                return;

            _recipeRows.Remove(row);
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            RecipeGrid.ItemsSource = null;
            RecipeGrid.ItemsSource = _recipeRows;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (_recipeRows.Any(r => r.RawMaterialId == 0))
            {
                MessageBox.Show("الرجاء اختيار مادة أولية لكل سطر.", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
               
                var existing = _db.ProductRecipes
                    .Where(pr => pr.ProductId == _productId)
                    .ToList();
                _db.ProductRecipes.RemoveRange(existing);

              
                foreach (var row in _recipeRows.Where(r => r.RawMaterialId != 0 && r.QuantiteNecessaire > 0))
                {
                    row.Id = 0; 
                    row.ProductId = _productId;
                    _db.ProductRecipes.Add(row);
                }

                _db.SaveChanges();

                MessageBox.Show("تم حفظ الوصفة بنجاح.", "تم",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الحفظ: {ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
