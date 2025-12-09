using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SuiviFinancier.Models;
using System.Threading.Tasks;
using System.Linq;

namespace SuiviFinancier.Controllers
{
    // ViewModel pour enrichir les données du Budget
    public class BudgetViewModel
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty; // Pour le visuel
        public string CategoryColor { get; set; } = string.Empty;
        public decimal LimitAmount { get; set; }
        public decimal SpentAmount { get; set; }
        
        public decimal RemainingAmount => LimitAmount - SpentAmount;
        public int Percentage => LimitAmount == 0 ? 0 : (int)((SpentAmount / LimitAmount) * 100);
    }

    public class BudgetController : Controller
    {
        private readonly AppDbContext _context;

        public BudgetController(AppDbContext context)
        {
            _context = context;
        }

        // --- LISTE ---
        public async Task<IActionResult> Index()
        {
            // 1. Définir la période (Ce mois-ci)
            var startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            // 2. Récupérer tous les budgets
            var budgets = await _context.Budgets.Include(b => b.Category).ToListAsync();
            
            // 3. Récupérer toutes les dépenses du mois (en une seule requête pour la performance)
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .ToListAsync();

            // 4. Construire la liste enrichie
            var budgetList = new List<BudgetViewModel>();

            foreach (var b in budgets)
            {
                // Somme des dépenses pour CETTE catégorie (filtrer par Type Depense)
                var spent = transactions
                    .Where(t => t.CategoryId == b.CategoryId && t.Category != null && t.Category.Type == "Depense")
                    .Sum(t => t.Amount);

                budgetList.Add(new BudgetViewModel
                {
                    Id = b.Id,
                    CategoryName = b.Category?.Name ?? "Sans catégorie",
                    CategoryIcon = b.Category?.Icon ?? "bi-tag",
                    CategoryColor = b.Category?.Color ?? "#6c757d",
                    LimitAmount = b.Amount,
                    SpentAmount = spent
                });
            }

            // 5. Trier par "Urgence" (Ceux qui sont le plus proche de la limite en premier)
            return View(budgetList.OrderByDescending(b => b.Percentage));
        }

        // --- CRÉER ---
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Amount,StartDate,EndDate,CategoryId")] Budget budget)
        {
            if (ModelState.IsValid)
            {
                budget.CreatedAt = DateTime.Now;
                _context.Add(budget);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", budget.CategoryId);
            return View(budget);
        }

        // --- MODIFIER ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", budget.CategoryId);
            return View(budget);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Amount,StartDate,EndDate,CategoryId")] Budget budget)
        {
            if (id != budget.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    budget.CreatedAt = DateTime.Now;
                    _context.Update(budget);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Budgets.Any(e => e.Id == budget.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", budget.CategoryId);
            return View(budget);
        }

        // --- SUPPRIMER ---
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var budget = await _context.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (budget == null) return NotFound();

            return View(budget);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget != null)
            {
                _context.Budgets.Remove(budget);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
