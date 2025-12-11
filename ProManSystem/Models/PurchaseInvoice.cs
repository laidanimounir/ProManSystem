using System;
using System.Collections.Generic;

namespace ProManSystem.Models
{
    public class PurchaseInvoice
    {
        public int Id { get; set; }
        public string NumeroFacture { get; set; } = string.Empty;   // 
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;

        public DateTime DateFacture { get; set; }

        public decimal MontantHT { get; set; }
        public decimal TauxTVA { get; set; }
        public decimal MontantTVA { get; set; }
        public decimal MontantTTC { get; set; }

        public decimal MontantPaye { get; set; }    
        public decimal Reste { get; set; }          
        public bool EstPayee { get; set; }          

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public List<PurchaseInvoiceLine> Lignes { get; set; } = new();
    }
}
