namespace ProManSystem.Models
{
    public class PurchaseInvoiceLine
    {
        public int Id { get; set; }

        public int PurchaseInvoiceId { get; set; }
        public PurchaseInvoice PurchaseInvoice { get; set; } = null!;

        public int RawMaterialId { get; set; }
        public RawMaterial RawMaterial { get; set; } = null!;

        public decimal Quantite { get; set; }
        public decimal PrixUnitaire { get; set; }
        public decimal MontantLigne { get; set; }   
    }
}
