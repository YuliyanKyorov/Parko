using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Parko.Data;

public sealed class ParkoDbContextFactory : IDesignTimeDbContextFactory<ParkoDbContext>
{
    public ParkoDbContext CreateDbContext(string[] args)
    {
        var configurationPath = Path.Combine(Directory.GetCurrentDirectory(), "Parko");
        if (!File.Exists(Path.Combine(configurationPath, "appsettings.json")))
        {
            configurationPath = Directory.GetCurrentDirectory();
        }

        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        environmentName = string.IsNullOrWhiteSpace(environmentName)
            ? Environments.Production
            : environmentName;

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(configurationPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddEnvironmentVariables();
        var configuration = configurationBuilder.Build();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Configure the DefaultConnection before creating a migration.");
        }

        var options = new DbContextOptionsBuilder<ParkoDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new ParkoDbContext(options);
    }
}
