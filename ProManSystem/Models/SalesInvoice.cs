using System;
using System.Collections.Generic;

namespace ProManSystem.Models
{
    public class SalesInvoice
    {
        public int Id { get; set; }
        public string NumeroFacture { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
        public DateTime DateFacture { get; set; }

       
        public RemiseType TypeRemise { get; set; } = RemiseType.Aucune;
        public decimal RemiseValeur { get; set; } = 0m;    
        public decimal RemiseMontant { get; set; } = 0m;     

       
        public decimal MontantHT { get; set; }               
        public decimal NetHT { get; set; }                   
        public decimal TauxTVA { get; set; }
        public decimal MontantTVA { get; set; }
        public decimal MontantTTC { get; set; }

      
        public ModeReglement ModeReglement { get; set; } = ModeReglement.Espece;

   
        public decimal MontantPaye { get; set; }
        public decimal Reste { get; set; }
        public bool EstPayee { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public List<SalesInvoiceLine> Lignes { get; set; } = new();
    }

   
    public enum RemiseType
    {
        Aucune = 0,        
        Pourcentage = 1,   
        Fixe = 2           
    }

    public enum ModeReglement
    {
        Espece = 0,            
        VersementBancaire = 1, 
        ATerme = 2,            
        Mixte = 3              
    }
}
