using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SuiviFinancier.Models;
using System.Threading.Tasks;
using System.Linq;

namespace SuiviFinancier.Controllers
{
    public class TransactionController : Controller
    {
        private readonly AppDbContext _context;

        public TransactionController(AppDbContext context)
        {
            _context = context;
        }

        // --- LISTE ---
        public async Task<IActionResult> Index()
        {
            // On inclut Category et Account pour pouvoir afficher leurs noms dans le tableau
            var transactions = _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .OrderByDescending(t => t.Date); // Trie par date décroissante (plus récent en haut)

            return View(await transactions.ToListAsync());
        }

        // --- CRÉER ---
        public IActionResult Create()
        {
            // On prépare les listes déroulantes
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Amount,Date,Type,CategoryId,AccountId")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                // 1. On enregistre la transaction (Comme avant)
                _context.Add(transaction);

                // --- DEBUT DU NOUVEAU CODE (LOGIQUE DE CALCUL) ---

                // 2. On va chercher le Compte et la Catégorie concernés dans la base
                var account = await _context.Accounts.FindAsync(transaction.AccountId);
                var category = await _context.Categories.FindAsync(transaction.CategoryId);

                // 3. On vérifie si tout existe bien
                if (account != null && category != null)
                {
                    // 4. On met à jour le solde selon le type
                    if (category.Type == "Depense")
                    {
                        // Si c'est une dépense, on retire de l'argent
                        account.Balance -= transaction.Amount; 
                    }
                    else if (category.Type == "Revenu")
                    {
                        // Si c'est un revenu, on ajoute de l'argent
                        account.Balance += transaction.Amount;
                    }

                    // 5. On dit à la base de données que le compte a été modifié
                    _context.Update(account);
                }
                // --- FIN DU NOUVEAU CODE ---

                // 6. On sauvegarde TOUT (la transaction ET le nouveau solde du compte)
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // Si le formulaire est invalide, on recharge les listes
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", transaction.CategoryId);
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Name", transaction.AccountId);
            return View(transaction);
        }

        // --- MODIFIER ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", transaction.CategoryId);
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Name", transaction.AccountId);
            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Amount,Date,Type,CategoryId,AccountId")] Transaction transaction)
        {
            if (id != transaction.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Transactions.Any(e => e.Id == transaction.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", transaction.CategoryId);
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Name", transaction.AccountId);
            return View(transaction);
        }

        // --- SUPPRIMER ---
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null) return NotFound();

            return View(transaction);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
