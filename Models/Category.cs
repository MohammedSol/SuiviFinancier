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

        // Navigation properties
        public ICollection<Transaction>? Transactions { get; set; }
        public ICollection<Budget>? Budgets { get; set; }
    }
}
