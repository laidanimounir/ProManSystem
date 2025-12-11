using System;

namespace ProManSystem.Models
{
    public class RawMaterial
    {
        public int Id { get; set; }

        public string CodeMatiere { get; set; } = string.Empty;   
        public string Designation { get; set; } = string.Empty;   

        public int? UnitId { get; set; }                          
        public Unit? Unit { get; set; }

        public decimal StockInitial { get; set; } = 0m;           
        public decimal StockActuel { get; set; } = 0m;            
        public decimal StockMin { get; set; } = 0m;               

        public decimal PMAPA { get; set; } = 0m;                  

        public DateTime DateCreation { get; set; } = DateTime.Now;
        public ICollection<ProductRecipe> ProductRecipes { get; set; } = new List<ProductRecipe>();

    }
}
