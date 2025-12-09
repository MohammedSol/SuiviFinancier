using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuiviFinancier.Models;
using System.Threading.Tasks;
using System.Linq;

namespace SuiviFinancier.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // --- LISTE ---
        public async Task<IActionResult> Index()
        {
            return View(await _context.Accounts.ToListAsync());
        }

        // --- CRÉER ---
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Balance,Type,AccountNumber,Currency,TargetAmount")] Account account)
        {
            if (ModelState.IsValid)
            {
                _context.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }

        // --- MODIFIER ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();
            return View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Balance,Type,AccountNumber,Currency,TargetAmount")] Account account)
        {
            if (id != account.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Accounts.Any(e => e.Id == account.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }

        // --- SUPPRIMER ---
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var account = await _context.Accounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null) return NotFound();

            return View(account);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
