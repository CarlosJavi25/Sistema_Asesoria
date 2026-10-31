using Microsoft.EntityFrameworkCore;
using Sanalia.Core.Entities;
using Sanalia.Core.Interfaces;
using Sanalia.Infrastructure.Data.Context;

namespace Sanalia.Infrastructure.Repositories
{
    public class CotizacionRepository(SanaliaDbContext db) : Repository<Cotizacion>(db), ICotizacionRepository
    {
        public async Task<IReadOnlyList<Cotizacion>> GetByAsesorAsync(
            int? idAsesor,
            CancellationToken ct = default) =>
            await IncludeDetails(Set.AsNoTracking())
                .Include(c => c.Asesoria)
                    .ThenInclude(a => a.Cliente)
                .Include(c => c.Asesoria)
                    .ThenInclude(a => a.Solicitud)
                        .ThenInclude(s => s.TipoSeguro)
                .Where(c => idAsesor == null || c.Asesoria.AsesorId == idAsesor)
                .OrderByDescending(c => c.FechaEmision)
                .ThenByDescending(c => c.Id)
                .AsSplitQuery()
                .ToListAsync(ct);

        public Task<Cotizacion?> GetConDetallesAsync(int idCotizacion, CancellationToken ct = default) =>
            IncludeDetails(Set).Include(c => c.Asesoria)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.Id == idCotizacion, ct);

        private static IQueryable<Cotizacion> IncludeDetails(IQueryable<Cotizacion> query) =>
            query.Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.Aseguradora)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.TipoSeguro)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.Coberturas);
    }
}