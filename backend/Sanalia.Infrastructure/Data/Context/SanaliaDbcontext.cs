using System.Net.WebSockets;
using Microsoft.EntityFrameworkCore;
using Sanalia.Core.Entities;

namespace Sanalia.Infrastructure.Data.Context;

public class SanaliaDbContext(DbContextOptions<SanaliaDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Asesor> Asesores => Set<Asesor>();
    public DbSet<Aseguradora> Aseguradoras => Set<Aseguradora>();
    public DbSet<TipoSeguro> TiposSeguro => Set<TipoSeguro>();
    public DbSet<ProductoSeguro> ProductosSeguro => Set<ProductoSeguro>();
    public DbSet<Cobertura> Coberturas => Set<Cobertura>();
    public DbSet<HorarioDisponible> Horarios => Set<HorarioDisponible>();
    public DbSet<SolicitudAsesoria> SolicitudesAsesoria => Set<SolicitudAsesoria>();
    public DbSet<SolicitudAsistencia> SolicitudesAsistencia => Set<SolicitudAsistencia>();
    public DbSet<Asesoria> Asesorias => Set<Asesoria>();
    public DbSet<Cotizacion> Cotizaciones => Set<Cotizacion>();
    public DbSet<DetalleCotizacion> DetallesCotizacion => Set<DetalleCotizacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SanaliaDbContext).Assembly);
    }
}