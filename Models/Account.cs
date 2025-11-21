using System.ComponentModel.DataAnnotations;

namespace SuiviFinancier.Models
{
    public class Account
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Bank, Cash, Credit Card, etc.

        [Required]
        [DataType(DataType.Currency)]
        public decimal Balance { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public User? User { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
    }
}
