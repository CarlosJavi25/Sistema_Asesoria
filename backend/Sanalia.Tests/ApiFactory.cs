using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Data.Sqlite;
using Sanalia.Infrastructure.Data.Context;

namespace Sanalia.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string Issuer = "Sanalia.Tests";
    public const string Audience = "Sanalia.Tests.Client";
    public const string SigningKey = "integration-test-signing-key-with-at-least-32-bytes";

    private readonly SqliteConnection connection = new("Data Source=:memory:");

    public ApiFactory() => connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" avoids loading the developer's user-secrets.
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = Issuer,
                ["Jwt:Audience"] = Audience,
                ["Jwt:SigningKey"] = SigningKey,
                ["Jwt:LifetimeMinutes"] = "30",
                ["ConnectionStrings:Default"] = "Data Source=:memory:"
            }));

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<SanaliaDbContext>();
            services.RemoveAll<DbContextOptions<SanaliaDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<SanaliaDbContext>>();
            services.AddDbContext<SanaliaDbContext>(o => o.UseSqlite(connection));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<SanaliaDbContext>().Database.EnsureCreated();
        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) connection.Dispose();
    }
}
