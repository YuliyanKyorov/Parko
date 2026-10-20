using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Parko.Data;
using Parko.Models;
using Parko.Services;

var b = WebApplication.CreateBuilder(args);
b.Services.AddControllersWithViews();
b.Services.AddDbContext<ParkoDbContext>(o =>
    o.UseSqlServer(b.Configuration.GetConnectionString("DefaultConnection")));
b.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
 {
     options.User.RequireUniqueEmail = true;
     options.Password.RequiredLength = b.Environment.IsDevelopment() ? 5 : 8;
     options.Password.RequireDigit = !b.Environment.IsDevelopment();
     options.Password.RequireLowercase = !b.Environment.IsDevelopment();
     options.Password.RequireUppercase = !b.Environment.IsDevelopment();
     options.Password.RequireNonAlphanumeric = !b.Environment.IsDevelopment();
     options.Lockout.MaxFailedAccessAttempts = 5;
     options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
 })
 .AddEntityFrameworkStores<ParkoDbContext>()
 .AddDefaultTokenProviders();
b.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});
b.Services.AddScoped<ParkingService>();
b.Services.AddScoped<IAccountService, AccountService>();
b.Services.AddScoped<IUserManagementService, UserManagementService>();

var app = b.Build();
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");
app.UseStatusCodePagesWithReExecute("/Error/{0}");
await using (var scope = app.Services.CreateAsyncScope())
{
    var services = scope.ServiceProvider;
    await services.GetRequiredService<ParkoDbContext>().Database.MigrateAsync();

    var adminPassword = b.Configuration["AdminAccount:Password"];
    if (string.IsNullOrWhiteSpace(adminPassword))
    {
        if (!b.Environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Configure AdminAccount:Password using a production secret before starting Parko.");
        }

        adminPassword = "admin";
    }

    await IdentitySeeder.EnsureAdministratorAsync(
        services.GetRequiredService<UserManager<ApplicationUser>>(),
        services.GetRequiredService<RoleManager<IdentityRole>>(),
        b.Configuration["AdminAccount:UserName"] ?? "admin",
        adminPassword);
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();