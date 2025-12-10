using System;
using System.ComponentModel.DataAnnotations;

namespace ProManSystem.Models
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string CodeFournisseur { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Designation { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Activite { get; set; }

        [MaxLength(300)]
        public string? Adresse { get; set; }

        [MaxLength(50)]
        public string? NumeroRC { get; set; }

        [MaxLength(50)]
        public string? MatriculeFiscal { get; set; }

        [MaxLength(20)]
        public string? TypeIdentification { get; set; }

        [MaxLength(50)]
        public string? NumeroIdentification { get; set; }

        
        public decimal? TotalAchats { get; set; }

        public decimal? Dette { get; set; }

        
        public bool EstActif { get; set; } = true;

        public DateTime DateCreation { get; set; } = DateTime.Now;
    }
}
