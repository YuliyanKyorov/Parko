using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Parko.Data;

namespace Parko.Tests;

internal sealed class SqliteParkoDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    private SqliteParkoDatabase(SqliteConnection connection, ParkoDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public ParkoDbContext Context { get; }

    public static async Task<SqliteParkoDatabase> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        connection.CreateFunction<string?, long?>(
            "DATALENGTH",
            value => value is null ? null : value.Length * sizeof(char));
        var options = new DbContextOptionsBuilder<ParkoDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new ParkoDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return new SqliteParkoDatabase(connection, context);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
