using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace Sanalia.Infrastructure.Data.Context;

public static class SanaliaDbContextConfigurator
{
    public static DbContextOptionsBuilder Configure(DbContextOptionsBuilder options, string connectionString) =>
        options
            .UseSqlite(connectionString)
            .UseSnakeCaseNamingConvention(); // fecha_registro, id_cliente... como en el diccionario de datos
}
public class SanaliaDbContextFactory : IDesignTimeDbContextFactory<SanaliaDbContext>
{
    public SanaliaDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SANALIA_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = DbPath.DefaultConnectionString();

        var builder = new DbContextOptionsBuilder<SanaliaDbContext>();
        SanaliaDbContextConfigurator.Configure(builder, connectionString);
        return new SanaliaDbContext(builder.Options);
    }
}
