using Microsoft.AspNetCore.Mvc;
using SuiviFinancier.Models;

namespace SuiviFinancier.Controllers
{
    public class BudgetController : Controller
    {
        private readonly AppDbContext _context;

        public BudgetController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Budget
        public IActionResult Index()
        {
            return View();
        }

        // GET: Budget/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Budget/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Budget budget)
        {
            if (ModelState.IsValid)
            {
                _context.Budgets.Add(budget);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(budget);
        }

        // GET: Budget/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var budget = _context.Budgets.Find(id);
            if (budget == null)
            {
                return NotFound();
            }
            return View(budget);
        }

        // POST: Budget/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Budget budget)
        {
            if (id != budget.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(budget);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(budget);
        }

        // GET: Budget/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var budget = _context.Budgets.Find(id);
            if (budget == null)
            {
                return NotFound();
            }

            return View(budget);
        }

        // POST: Budget/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var budget = _context.Budgets.Find(id);
            if (budget != null)
            {
                _context.Budgets.Remove(budget);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
