using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Parko.Data;
using Parko.Models;

namespace Parko.Tests;

internal sealed class IdentityTestHost : IAsyncDisposable
{
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly ServiceProvider _services;

    private IdentityTestHost(
        Microsoft.Data.Sqlite.SqliteConnection connection,
        ServiceProvider services)
    {
        _connection = connection;
        _services = services;
    }

    public IServiceScope CreateScope() => _services.CreateScope();

    public static async Task<IdentityTestHost> CreateAsync()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        connection.CreateFunction<string?, long?>(
            "DATALENGTH",
            value => value is null ? null : value.Length * sizeof(char));
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ParkoDbContext>(options => options.UseSqlite(connection));
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ParkoDbContext>();

        var provider = services.BuildServiceProvider();
        await using (var scope = provider.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<ParkoDbContext>()
                .Database.EnsureCreatedAsync();
        }

        return new IdentityTestHost(connection, provider);
    }

    public async ValueTask DisposeAsync()
    {
        await _services.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
