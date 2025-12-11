namespace ProManSystem.Models
{
    public class ConsumptionResult
    {
        public int RawMaterialId { get; set; }
        public string RawMaterialName { get; set; } = string.Empty;

        public decimal CurrentStock { get; set; }       
        public decimal QuantityNeeded { get; set; }     
        public decimal StockAfter { get; set; }         
    }
}
