using System.Collections.Generic;

namespace SuiviFinancier.Models
{
    public class DashboardViewModel
    {
        // Indicateurs globaux
        public decimal TotalBalance { get; set; }
        public decimal IncomeThisMonth { get; set; }
        public decimal ExpenseThisMonth { get; set; }

        // Pour le graphique (Dépenses par catégorie)
        public string[] ExpenseCategoryLabels { get; set; } = Array.Empty<string>();
        public decimal[] ExpenseCategoryData { get; set; } = Array.Empty<decimal>();

        // Liste des dernières transactions
        public List<Transaction> RecentTransactions { get; set; } = new List<Transaction>();
    }
}
