using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Parko.Data;
using Parko.Models;
using Parko.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ParkoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = builder.Environment.IsDevelopment() ? 5 : 8;
    options.Password.RequireDigit = !builder.Environment.IsDevelopment();
    options.Password.RequireLowercase = !builder.Environment.IsDevelopment();
    options.Password.RequireUppercase = !builder.Environment.IsDevelopment();
    options.Password.RequireNonAlphanumeric = !builder.Environment.IsDevelopment();
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
})
.AddEntityFrameworkStores<ParkoDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});

builder.Services.AddScoped<ParkingService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStatusCodePagesWithReExecute("/Error/Index/{0}");

await using (var scope = app.Services.CreateAsyncScope())
{
    var services = scope.ServiceProvider;
    await services.GetRequiredService<ParkoDbContext>().Database.MigrateAsync();

    var adminPassword = builder.Configuration["AdminAccount:Password"];
    if (string.IsNullOrWhiteSpace(adminPassword))
    {
        if (!builder.Environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Configure AdminAccount:Password using a production secret before starting Parko.");
        }

        adminPassword = "admin";
    }

    await IdentitySeeder.EnsureAdministratorAsync(
        services.GetRequiredService<UserManager<ApplicationUser>>(),
        services.GetRequiredService<RoleManager<IdentityRole>>(),
        builder.Configuration["AdminAccount:UserName"] ?? "admin",
        adminPassword);
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();