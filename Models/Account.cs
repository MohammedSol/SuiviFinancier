using System.ComponentModel.DataAnnotations;

namespace SuiviFinancier.Models
{
    public class Account
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // --- NOUVEAU : Numéro de compte (ex: RIB ou 4 derniers chiffres) ---
        [StringLength(20)]
        public string AccountNumber { get; set; } = "0000";

        // --- NOUVEAU : Devise (MAD, EUR, USD) ---
        [StringLength(10)]
        public string Currency { get; set; } = "MAD";

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Courant, Epargne, Especes

        [Required]
        [DataType(DataType.Currency)]
        public decimal Balance { get; set; }

        // --- NOUVEAU : Objectif (Pour les comptes Épargne uniquement) ---
        // Nullable (decimal?) car un compte courant n'a pas forcément d'objectif
        [DataType(DataType.Currency)]
        public decimal? TargetAmount { get; set; }

        public int? UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public User? User { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
    }
}
