using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sanalia.Core.Interfaces;
using Sanalia.Infrastructure.Data.Security;
using Sanalia.Infrastructure.Data.Seeding;
using Sanalia.Infrastructure.Repositories;
using Sanalia.Infrastructure.Services;
using Sanalia.Core.Interfaces.IServices;

namespace Sanalia.Infrastructure.Data.Context;

public static class DI
{
    /// Aplica las migraciones, activa el modo WAL de SQLite y carga los datos de prueba.

    public static async Task DbInitializer(
            this IServiceProvider services, bool seedData = true, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SanaliaDbContext>();

        if (!db.Database.GetMigrations().Any())
        {
            throw new InvalidOperationException(
                "No hay migraciones en Sanalia.Infrastructure. Créalas una vez desde backend con: " +
                "dotnet tool restore && dotnet ef migrations add Inicial " +
                "--project Sanalia.Infrastructure --startup-project Sanalia.Api");
        }

        await db.Database.MigrateAsync(ct);

        // WAL: las lecturas no se bloquean mientras alguien escribe. Queda guardado en el archivo .db.
        await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;", ct);

        if (seedData)
            await scope.ServiceProvider.GetRequiredService<IDataSeeder>().SeedAsync(ct);
    }

    /// <summary>
    /// Registra el DbContext (SQLite), los repositorios, el hasher de contraseñas y el seeder.
    /// </summary>

    public static IServiceCollection AddSanaliaData(this IServiceCollection services, string? connectionString = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = DbPath.DefaultConnectionString();

        services.AddDbContext<SanaliaDbContext>(options =>
            SanaliaDbContextConfigurator.Configure(options, connectionString));

        // Repositorio genérico para las entidades que no necesitan consultas propias
        // (Aseguradora, TipoSeguro, Cobertura, DetalleCotizacion).
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IAsesorRepository, AsesorRepository>();
        services.AddScoped<IProductoSeguroRepository, ProductoSeguroRepository>();
        services.AddScoped<IHorarioDisponibleRepository, HorarioDisponibleRepository>();
        services.AddScoped<ISolicitudAsesoriaRepository, SolicitudAsesoriaRepository>();
        services.AddScoped<ISolicitudAsistenciaRepository, SolicitudAsistenciaRepository>();
        services.AddScoped<IAsesoriaRepository, AsesoriaRepository>();
        services.AddScoped<ICotizacionRepository, CotizacionRepository>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IDataSeeder, DataSeeder>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductoSeguroService, ProductoSeguroService>();
        services.AddScoped<IHorarioDisponibleService, HorarioDisponibleService>();
        services.AddScoped<ISolicitudAsesoriaService, SolicitudAsesoriaService>();
        services.AddScoped<ISolicitudAsistenciaService, SolicitudAsistenciaService>();
        services.AddScoped<IAsesoriaService, AsesoriaService>();
        services.AddScoped<ICotizacionService, CotizacionService>();

        return services;
    }

}
