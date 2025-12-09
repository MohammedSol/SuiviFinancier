using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Nécessaire pour [NotMapped]
using Microsoft.AspNetCore.Http; // Nécessaire pour IFormFile

namespace SuiviFinancier.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string Type { get; set; } = string.Empty; // Income or Expense

        public int? AccountId { get; set; }
        public int? CategoryId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- NOUVEAU : GESTION DES FICHIERS ---

        // 1. Stocke le chemin d'accès (ex: "/uploads/mon-recu.jpg")
        [StringLength(500)]
        public string? ReceiptPath { get; set; }

        // 2. Reçoit le fichier envoyé par le formulaire (Pas stocké en Base)
        [NotMapped]
        public IFormFile? ReceiptFile { get; set; }

        // Navigation properties
        public Account? Account { get; set; }
        public Category? Category { get; set; }
    }
}
