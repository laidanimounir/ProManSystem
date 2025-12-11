using System.Collections.Generic;
using System.Linq;
using ProManSystem.Data;
using ProManSystem.Models;

namespace ProManSystem.Services
{
    public class ProductionService
    {
        private readonly AppDbContext _db;

        public ProductionService(AppDbContext db)
        {
            _db = db;
        }

      
        public List<ConsumptionResult> CalculateConsumption(int productId, decimal quantityToProduce)
        {
          
            var recipes = _db.ProductRecipes
                .Where(pr => pr.ProductId == productId)
                .Select(pr => new
                {
                    pr.RawMaterialId,
                    pr.QuantiteNecessaire,
                    pr.RawMaterial.Designation,
                    pr.RawMaterial.StockActuel
                })
                .ToList();

            var results = new List<ConsumptionResult>();

            foreach (var r in recipes)
            {
                var needed = r.QuantiteNecessaire * quantityToProduce;
                var after = r.StockActuel - needed;

                results.Add(new ConsumptionResult
                {
                    RawMaterialId = r.RawMaterialId,
                    RawMaterialName = r.Designation,
                    CurrentStock = r.StockActuel,
                    QuantityNeeded = needed,
                    StockAfter = after
                });
            }

            return results;
        }
    }
}
