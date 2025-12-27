using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProManSystem.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string CodeProduit { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Nom { get; set; } = string.Empty;

        public decimal PrixVente { get; set; }

        public decimal StockActuel { get; set; }

        public decimal StockMin { get; set; }

        public decimal CoutProduction { get; set; }

        public decimal Marge { get; set; }

        public DateTime DateCreation { get; set; }

        public ICollection<ProductRecipe> ProductRecipes { get; set; } = new List<ProductRecipe>();
    }
}
