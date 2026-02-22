using asprule1020.Areas.Identity.Pages.Account;
using asprule1020.DataAccess.Data;
using asprule1020.DataAccess.Repository;
using asprule1020.DataAccess.Repository.IRepository;
using asprule1020.Infrastruture.Identity;
using asprule1020.Models;
using asprule1020.Utility;
using BulkyBook.DataAccess.DbInitializer;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = builder.Configuration["Authentication:Facebook:AppId"] ?? string.Empty;
    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"] ?? string.Empty;
});
// Session Configuration - Add Session Services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>,
    BcryptPasswordHasher<ApplicationUser>>();
builder.Services.AddScoped<
    IUserClaimsPrincipalFactory<ApplicationUser>,
    AppClaimsPrincipalFactory>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Admin/Identity/Account/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
SeedDatabase();
app.MapRazorPages();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Client}/{controller=Register}/{action=Index}/{id?}");

app.Run();

void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        //var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        //if (!roleManager.RoleExistsAsync(SD.Role_Client).GetAwaiter().GetResult())
        //{
        //    roleManager.CreateAsync(new IdentityRole(SD.Role_Client)).GetAwaiter().GetResult();
        //    roleManager.CreateAsync(new IdentityRole(SD.Role_Evaluator)).GetAwaiter().GetResult();
        //    roleManager.CreateAsync(new IdentityRole(SD.Role_Po_Head)).GetAwaiter().GetResult();
        //    roleManager.CreateAsync(new IdentityRole(SD.Role_Region_Focal)).GetAwaiter().GetResult();
        //    roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
        //}
        
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}