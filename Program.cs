using Microsoft.EntityFrameworkCore;
using SuiviFinancier.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using SuiviFinancier.ML;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Politique globale : verrouillage de tout le site (sauf pages Identity)
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// Add DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add ML.NET Category Predictor Service
builder.Services.AddSingleton<CategoryPredictorService>();

// Add Identity services
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // Simplified password requirements for development
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<AppDbContext>();

// Configure application cookies for authentication redirects
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Configure Razor Pages for Identity (sans autorisation globale)
builder.Services.AddRazorPages(options =>
{
    // Permettre l'accès anonyme aux pages d'authentification
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Register");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Logout");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/ForgotPassword");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/ResetPassword");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // Must be before UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages(); // For Identity UI pages (Login, Register, etc.)

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
