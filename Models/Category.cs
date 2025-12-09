using System.ComponentModel.DataAnnotations;

namespace SuiviFinancier.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Type { get; set; } = string.Empty; // Income or Expense

        // --- NOUVEAU : Identité Visuelle ---
        // Stocke le nom de la classe Bootstrap Icon (ex: "bi-cart-fill")
        [StringLength(50)]
        public string Icon { get; set; } = "bi-tag";
        
        // Stocke le code Hexadécimal de la couleur (ex: "#FF5733")
        [StringLength(7)]
        public string Color { get; set; } = "#6c757d"; // Gris par défaut

        // Navigation properties
        public ICollection<Transaction>? Transactions { get; set; }
        public ICollection<Budget>? Budgets { get; set; }
    }
}
