using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuiviFinancier.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SuiviFinancier.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
    {
        // 1. Définir les dates par défaut (Ce mois-ci) si rien n'est envoyé
        if (!startDate.HasValue)
        {
            startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }
        if (!endDate.HasValue)
        {
            endDate = startDate.Value.AddMonths(1).AddDays(-1);
        }

        // 2. Récupérer les transactions FILTRÉES par ces dates
        var transactionsFiltered = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .ToListAsync();

        // 3. Calculer les totaux sur ces données filtrées
        var income = transactionsFiltered
            .Where(t => t.Category != null && t.Category.Type == "Revenu")
            .Sum(t => t.Amount);

        var expense = transactionsFiltered
            .Where(t => t.Category != null && t.Category.Type == "Depense")
            .Sum(t => t.Amount);

        // 4. Préparer les données du Graphique (toujours sur les données filtrées)
        var expenseByCat = transactionsFiltered
            .Where(t => t.Category != null && t.Category.Type == "Depense")
            .GroupBy(t => t.Category!.Name)
            .Select(g => new { 
                Category = g.Key, 
                Total = g.Sum(t => t.Amount) 
            })
            .ToList();

        // --- NOUVEAU : PRÉPARATION DU LINE CHART (Évolution journalière) ---
        
        // 1. On crée une liste de tous les jours de la période sélectionnée
        // Cela évite d'avoir des trous dans le graphique si un jour n'a pas de transaction
        var days = new List<DateTime>();
        for (var date = startDate.Value; date <= endDate.Value; date = date.AddDays(1))
        {
            days.Add(date);
        }

        // 2. On prépare les tableaux vides
        var labels = new List<string>();
        var incomeData = new List<decimal>();
        var expenseData = new List<decimal>();

        // 3. Pour chaque jour, on calcule la somme
        foreach (var day in days)
        {
            labels.Add(day.ToString("dd/MM")); // Axe X : Le jour et le mois

            // Somme des revenus pour ce jour précis
            var dayIncome = transactionsFiltered
                .Where(t => t.Date.Date == day.Date && t.Category != null && t.Category.Type == "Revenu")
                .Sum(t => t.Amount);
            
            // Somme des dépenses pour ce jour précis
            var dayExpense = transactionsFiltered
                .Where(t => t.Date.Date == day.Date && t.Category != null && t.Category.Type == "Depense")
                .Sum(t => t.Amount);

            incomeData.Add(dayIncome);
            expenseData.Add(dayExpense);
        }

        // 5. Solde Total (Lui, il ne dépend pas des dates, c'est le solde actuel réel des comptes)
        var totalBalance = await _context.Accounts.SumAsync(a => (decimal?)a.Balance) ?? 0;

        // --- NOUVEAU : CALCUL DES BUDGETS ---
        
        // 1. On récupère tous les budgets définis en base
        var allBudgets = await _context.Budgets.Include(b => b.Category).ToListAsync();
        var budgetStatuses = new List<BudgetStatus>();

        foreach (var budget in allBudgets)
        {
            if (budget.Category != null)
            {
                // 2. On calcule combien on a dépensé dans CETTE catégorie pour la PÉRIODE choisie
                var spentInCategory = transactionsFiltered
                    .Where(t => t.CategoryId == budget.CategoryId && t.Category != null && t.Category.Type == "Depense")
                    .Sum(t => t.Amount);

                // 3. On crée l'objet statut
                budgetStatuses.Add(new BudgetStatus
                {
                    CategoryName = budget.Category.Name,
                    LimitAmount = budget.Amount,
                    SpentAmount = spentInCategory
                });
            }
        }

        // 4. On ordonne pour voir les plus critiques en premier (ceux qui dépassent le plus)
        budgetStatuses = budgetStatuses.OrderByDescending(b => b.Percentage).ToList();

        // 6. Remplir le ViewModel
        var vm = new DashboardViewModel
        {
            StartDate = startDate.Value,
            EndDate = endDate.Value,
            TotalBalance = totalBalance,
            Income = income,
            Expense = expense,
            RecentTransactions = transactionsFiltered.OrderByDescending(t => t.Date).Take(5).ToList(),
            ExpenseCategoryLabels = expenseByCat.Select(x => x.Category).ToArray(),
            ExpenseCategoryData = expenseByCat.Select(x => x.Total).ToArray(),
            
            // --- NOUVELLES PROPRIÉTÉS ---
            SplineChartLabels = labels.ToArray(),
            IncomeDailyData = incomeData.ToArray(),
            ExpenseDailyData = expenseData.ToArray(),
            
            // --- NOUVELLE PROPRIÉTÉ BUDGETS ---
            BudgetStatuses = budgetStatuses
        };

        return View(vm);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
