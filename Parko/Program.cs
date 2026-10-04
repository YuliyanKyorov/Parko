using Microsoft.EntityFrameworkCore;
using Parko.Data;
using Parko.Services;

var b = WebApplication.CreateBuilder(args);
b.Services.AddControllersWithViews();
b.Services.AddDbContext<ParkoDbContext>(o =>
    o.UseSqlServer(b.Configuration.GetConnectionString("DefaultConnection")));
b.Services.AddScoped<ParkingService>();// ВНИМАНИЕ: регистрираме сервиза.
var app = b.Build();
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");          // за грешките
app.UseStatusCodePagesWithReExecute("/Error/{0}"); 
using (var s = app.Services.CreateScope())
    s.ServiceProvider.GetRequiredService<ParkoDbContext>().Database.Migrate();   // ВНИМАНИЕ: прилага миграциите
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Parking}/{action=Index}/{id?}");
app.Run();
//Как да го тествам пусни проекта и отвори адрес, който не съществува, например /Parking/Nqma. Ще видиш „404 – Страницата не е намерена“. 