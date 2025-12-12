using Microsoft.AspNetCore.Mvc;
using SuiviFinancier.ML;
using System;
using System.IO;

namespace SuiviFinancier.Controllers
{
    public class MLController : Controller
    {
        private readonly CategoryPredictorService _predictorService;

        public MLController(CategoryPredictorService predictorService)
        {
            _predictorService = predictorService;
        }

        // GET: /ML/Train
        public IActionResult Train()
        {
            try
            {
                string dataPath = Path.Combine(Directory.GetCurrentDirectory(), "MLData", "training-data.csv");
                
                if (!System.IO.File.Exists(dataPath))
                {
                    return Content($"Fichier de données introuvable: {dataPath}");
                }

                _predictorService.TrainModel(dataPath);
                
                return Content("✅ Modèle ML entraîné avec succès! Le fichier 'category-model.zip' a été créé dans MLData/");
            }
            catch (Exception ex)
            {
                return Content($"❌ Erreur lors de l'entraînement: {ex.Message}\n\n{ex.StackTrace}");
            }
        }

        // GET: /ML/Test?text=Courses Carrefour
        public IActionResult Test(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Content("Usage: /ML/Test?text=Courses Carrefour");
            }

            try
            {
                string prediction = _predictorService.PredictCategory(text);
                return Content($"📊 Texte: '{text}'\n✅ Catégorie prédite: {prediction}");
            }
            catch (Exception ex)
            {
                return Content($"❌ Erreur: {ex.Message}\n\nAvez-vous entraîné le modèle? Allez sur /ML/Train d'abord.");
            }
        }
    }
}
