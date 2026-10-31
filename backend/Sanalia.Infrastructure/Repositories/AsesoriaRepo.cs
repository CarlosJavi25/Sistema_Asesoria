using Microsoft.EntityFrameworkCore;
using Sanalia.Core.Entities;
using Sanalia.Core.Interfaces;
using Sanalia.Infrastructure.Data.Context;

namespace Sanalia.Infrastructure.Repositories
{
    public class AsesoriaRepository(SanaliaDbContext db) : Repository<Asesoria>(db), IAsesoriaRepository
    {
        public Task<Asesoria?> GetBySolicitudAsync(int idSolicitud, CancellationToken ct = default) =>
            Set.AsNoTracking()
                .Include(a => a.Cotizaciones)
                    .ThenInclude(c => c.Detalles)
                        .ThenInclude(d => d.Producto)
                            .ThenInclude(p => p.Aseguradora)
                .Include(a => a.Cotizaciones)
                    .ThenInclude(c => c.Detalles)
                        .ThenInclude(d => d.Producto)
                            .ThenInclude(p => p.TipoSeguro)
                .Include(a => a.Cotizaciones)
                    .ThenInclude(c => c.Detalles)
                        .ThenInclude(d => d.Producto)
                            .ThenInclude(p => p.Coberturas)
                .AsSplitQuery()
                .FirstOrDefaultAsync(a => a.SolicitudId == idSolicitud, ct);

        public async Task<IReadOnlyList<Asesoria>> GetByAsesorAsync(
            int? idAsesor,
            CancellationToken ct = default) =>
            await IncludeQuoteDetails(Set.AsNoTracking())
                .Include(a => a.Cliente)
                .Include(a => a.Solicitud)
                    .ThenInclude(s => s.TipoSeguro)
                .Where(a => idAsesor == null || a.AsesorId == idAsesor)
                .OrderByDescending(a => a.FechaAsesoria)
                .AsSplitQuery()
                .ToListAsync(ct);

        public async Task<IReadOnlyList<Asesoria>> GetHistorialPorClienteAsync(int idCliente, CancellationToken ct = default) =>
            await Set.AsNoTracking()
                .Include(a => a.Asesor)
                .Include(a => a.Cotizaciones)
                    .ThenInclude(c => c.Detalles)
                        .ThenInclude(d => d.Producto)
                            .ThenInclude(p => p.Aseguradora)
                .Include(a => a.Cotizaciones)
                    .ThenInclude(c => c.Detalles)
                        .ThenInclude(d => d.Producto)
                            .ThenInclude(p => p.TipoSeguro)
                .Include(a => a.Cotizaciones)
                    .ThenInclude(c => c.Detalles)
                        .ThenInclude(d => d.Producto)
                            .ThenInclude(p => p.Coberturas)
                .Where(a => a.ClienteId == idCliente)
                .OrderByDescending(a => a.FechaAsesoria)
                .AsSplitQuery()
                .ToListAsync(ct);

        private static IQueryable<Asesoria> IncludeQuoteDetails(IQueryable<Asesoria> query) =>
            query.Include(a => a.Cotizaciones)
                    .ThenInclude(c => c.Detalles)
                        .ThenInclude(d => d.Producto)
                            .ThenInclude(p => p.Aseguradora)
                .Include(a => a.Cotizaciones)
                    .ThenInclude(c => c.Detalles)
                        .ThenInclude(d => d.Producto)
                            .ThenInclude(p => p.TipoSeguro)
                .Include(a => a.Cotizaciones)
                    .ThenInclude(c => c.Detalles)
                        .ThenInclude(d => d.Producto)
                            .ThenInclude(p => p.Coberturas);
    }
}