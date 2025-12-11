using System.ComponentModel.DataAnnotations;

namespace ProManSystem.Models
{
    public class ProductRecipe
    {
        public int Id { get; set; }

       
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

      
        public int RawMaterialId { get; set; }
        public RawMaterial RawMaterial { get; set; } = null!;

       
        public decimal QuantiteNecessaire { get; set; }
    }
}
