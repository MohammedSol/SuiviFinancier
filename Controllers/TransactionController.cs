using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SuiviFinancier.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Hosting; // Pour accéder à wwwroot

namespace SuiviFinancier.Controllers
{
    public class TransactionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment; // Pour savoir où est wwwroot

        public TransactionController(AppDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
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
        public async Task<IActionResult> Create([Bind("Id,Description,Amount,Date,Type,CategoryId,AccountId,ReceiptFile")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                // --- LOGIQUE D'UPLOAD DE FICHIER ---
                if (transaction.ReceiptFile != null)
                {
                    // 1. Définir le dossier de destination (wwwroot/uploads)
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Path.GetFileNameWithoutExtension(transaction.ReceiptFile.FileName);
                    string extension = Path.GetExtension(transaction.ReceiptFile.FileName);
                    
                    // 2. Créer un nom unique (NomFichier + Date + Extension)
                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    
                    // 3. Créer le dossier s'il n'existe pas
                    string path = Path.Combine(wwwRootPath + "/uploads/");
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                    string fullPath = Path.Combine(path + fileName);

                    // 4. Copier le fichier
                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await transaction.ReceiptFile.CopyToAsync(fileStream);
                    }

                    // 5. Sauvegarder le chemin relatif dans la base
                    transaction.ReceiptPath = "/uploads/" + fileName;
                }
                // ------------------------------------

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

        // --- EXPORT CSV ---
        public async Task<IActionResult> Export()
        {
            // 1. Récupérer toutes les transactions
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            // 2. Créer le contenu du fichier (StringBuilder est très rapide pour ça)
            var builder = new StringBuilder();

            // 3. Ajouter la ligne d'en-tête (Les titres des colonnes)
            // On utilise le point-virgule ";" pour la compatibilité Excel FR
            builder.AppendLine("Date;Description;Catégorie;Compte;Type;Montant");

            // 4. Parcourir les données et ajouter les lignes
            foreach (var t in transactions)
            {
                // On prépare les données (gestion des nulls avec "?")
                var date = t.Date.ToShortDateString();
                var description = t.Description?.Replace(";", ",").Replace("\r", " ").Replace("\n", " ") ?? "";
                var categorie = t.Category?.Name ?? "Aucune";
                var compte = t.Account?.Name ?? "Aucun";
                var type = t.Category?.Type ?? "N/A";
                
                // Pour le montant : positif si revenu, négatif si dépense
                // On force le format numérique français (avec virgule)
                decimal montantReel = t.Amount;
                if (type == "Depense") montantReel = -t.Amount;
                else if (type == "Revenu") montantReel = t.Amount;

                // On écrit la ligne
                builder.AppendLine($"{date};{description};{categorie};{compte};{type};{montantReel}");
            }

            // 5. Renvoyer le fichier au navigateur
            // L'encodage UTF-8 avec Preamble permet à Excel de reconnaître les accents
            return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray(), 
                        "text/csv", 
                        $"Transactions_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }
    }
}
