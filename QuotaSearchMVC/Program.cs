using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuotaSearchMVC.Data;
using QuotaSearchMVC.Models;
using QuotaSearchMVC.Services;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultUI(); // Login/Register UI otomatik gelir

// MVC + Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// QuotaService
builder.Services.AddScoped<IQuotaService, QuotaService>();

var app = builder.Build();

// Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Search}/{action=Index}/{id?}");

app.MapRazorPages(); // Identity UI sayfalarý için þart

app.Run();
