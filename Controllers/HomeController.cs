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

    public async Task<IActionResult> Index()
    {
        // 1. Calculer le solde total de tous les comptes
        var totalBalance = await _context.Accounts.SumAsync(a => (decimal?)a.Balance) ?? 0;

        // 2. Définir la période (Ce mois-ci)
        var startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // 3. Récupérer les transactions du mois
        var transactionsThisMonth = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .ToListAsync();

        // 4. Calculer Revenus et Dépenses
        var income = transactionsThisMonth
            .Where(t => t.Category != null && t.Category.Type == "Revenu")
            .Sum(t => t.Amount);

        var expense = transactionsThisMonth
            .Where(t => t.Category != null && t.Category.Type == "Depense")
            .Sum(t => t.Amount);

        // 5. Préparer les données pour le GRAPHIQUE (Dépenses par catégorie)
        var expenseByCat = transactionsThisMonth
            .Where(t => t.Category != null && t.Category.Type == "Depense")
            .GroupBy(t => t.Category!.Name)
            .Select(g => new { 
                Category = g.Key, 
                Total = g.Sum(t => t.Amount) 
            })
            .ToList();

        // 6. Récupérer les 5 dernières transactions pour l'affichage
        var recent = await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .OrderByDescending(t => t.Date)
            .Take(5)
            .ToListAsync();

        // 7. Remplir le ViewModel
        var vm = new DashboardViewModel
        {
            TotalBalance = totalBalance,
            IncomeThisMonth = income,
            ExpenseThisMonth = expense,
            RecentTransactions = recent,
            // Conversion en tableaux pour JavaScript
            ExpenseCategoryLabels = expenseByCat.Select(x => x.Category).ToArray(),
            ExpenseCategoryData = expenseByCat.Select(x => x.Total).ToArray()
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
