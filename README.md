# SuiviFinancier - Application de Suivi Financier

Application ASP.NET Core MVC pour gérer vos finances personnelles.

## 🚀 Fonctionnalités

- **Gestion des Utilisateurs** : Créer et gérer les profils utilisateurs
- **Gestion des Comptes** : Suivre plusieurs comptes (banque, espèces, carte de crédit, etc.)
- **Gestion des Transactions** : Enregistrer les revenus et dépenses
- **Gestion des Budgets** : Définir et suivre des budgets par catégorie
- **Catégorisation** : Organiser les transactions par catégories

## 📋 Prérequis

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server LocalDB](https://learn.microsoft.com/fr-fr/sql/database-engine/configure-windows/sql-server-express-localdb) ou SQL Server
- Un éditeur de code (Visual Studio, Visual Studio Code, ou Rider)

## 🏗️ Structure du Projet

```
SuiviFinancier/
│
├── Controllers/
│   ├── HomeController.cs
│   ├── TransactionController.cs
│   ├── BudgetController.cs
│   ├── AccountController.cs
│   └── UserController.cs
│
├── Models/
│   ├── User.cs
│   ├── Account.cs
│   ├── Category.cs
│   ├── Transaction.cs
│   ├── Budget.cs
│   └── AppDbContext.cs
│
├── Views/
│   ├── Home/
│   ├── Transactions/
│   ├── Budgets/
│   ├── Accounts/
│   └── Shared/
│
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── images/
│
├── appsettings.json
└── Program.cs
```

## 🔧 Installation

1. **Cloner ou télécharger le projet**

2. **Naviguer vers le répertoire du projet**
   ```bash
   cd SuiviFin
   ```

3. **Restaurer les packages NuGet**
   ```bash
   dotnet restore
   ```

4. **Configurer la base de données**
   
   La chaîne de connexion se trouve dans `appsettings.json` :
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SuiviFinancierDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

5. **Créer et appliquer les migrations**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

6. **Compiler le projet**
   ```bash
   dotnet build
   ```

7. **Lancer l'application**
   ```bash
   dotnet run
   ```

8. **Accéder à l'application**
   
   Ouvrir votre navigateur et aller à : `https://localhost:5001` ou `http://localhost:5000`

## 📦 Packages NuGet Utilisés

- **Microsoft.EntityFrameworkCore.SqlServer** (v9.0.0) - Provider SQL Server pour Entity Framework Core
- **Microsoft.EntityFrameworkCore.Tools** (v9.0.0) - Outils pour les migrations EF Core

## 🗄️ Modèle de Données

### User (Utilisateur)
- Id, Name, Email, Password, CreatedAt
- Relations : Accounts, Budgets

### Account (Compte)
- Id, Name, Type, Balance, UserId, CreatedAt
- Relations : User, Transactions

### Category (Catégorie)
- Id, Name, Description, Type (Income/Expense)
- Relations : Transactions, Budgets

### Transaction
- Id, Description, Amount, Date, Type, AccountId, CategoryId, CreatedAt
- Relations : Account, Category

### Budget
- Id, Name, Amount, StartDate, EndDate, UserId, CategoryId, CreatedAt
- Relations : User, Category

## 🛠️ Commandes Utiles

### Entity Framework Core

```bash
# Créer une nouvelle migration
dotnet ef migrations add <NomMigration>

# Appliquer les migrations
dotnet ef database update

# Supprimer la dernière migration
dotnet ef migrations remove

# Voir la liste des migrations
dotnet ef migrations list
```

### Développement

```bash
# Lancer en mode développement avec rechargement automatique
dotnet watch run

# Nettoyer les fichiers de build
dotnet clean

# Publier l'application
dotnet publish -c Release
```

## 🎨 Technologies Utilisées

- **ASP.NET Core 9.0** - Framework web
- **Entity Framework Core 9.0** - ORM pour l'accès aux données
- **SQL Server** - Base de données
- **Bootstrap 5** - Framework CSS (inclus par défaut)
- **MVC Pattern** - Architecture Modèle-Vue-Contrôleur

## 📝 Remarques

- Le projet utilise **LocalDB** par défaut. Pour utiliser un autre serveur SQL Server, modifiez la chaîne de connexion dans `appsettings.json`
- Les mots de passe sont actuellement stockés en texte brut. Pour un environnement de production, utilisez **ASP.NET Core Identity** avec hachage de mots de passe
- Les validations de base sont implémentées avec des Data Annotations

## 🚧 Développements Futurs

- [ ] Authentification et autorisation avec Identity
- [ ] Tableaux de bord et graphiques
- [ ] Rapports financiers
- [ ] Export des données (PDF, Excel)
- [ ] API REST
- [ ] Application mobile

## 📄 Licence

Ce projet est à usage éducatif.

## 👨‍💻 Auteur

Projet réalisé dans le cadre du cours .NET - EMSI S3
