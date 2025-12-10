using System.ComponentModel.DataAnnotations;

namespace ProManSystem.Models
{
    public class Customer
    {
        [Key]                
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string CodeClient { get; set; } = "";  

        [Required]
        [MaxLength(120)]
        public string NomComplet { get; set; } = "";  

        [MaxLength(80)]
        public string? Activite { get; set; }

        [MaxLength(200)]
        public string? Adresse { get; set; }

        [MaxLength(50)]
        public string? NumeroRC { get; set; }

        [MaxLength(50)]
        public string? MatriculeFiscal { get; set; }

        [MaxLength(30)]
        public string? TypeIdentification { get; set; }  

        [MaxLength(60)]
        public string? NumeroIdentification { get; set; }

        public decimal? CA_HT { get; set; }     
        public decimal? TauxTVA { get; set; }   
        public decimal? CA_TTC { get; set; }

        public bool EstRadie { get; set; }
        public DateTime? DateRadiation { get; set; }  

    }
}
