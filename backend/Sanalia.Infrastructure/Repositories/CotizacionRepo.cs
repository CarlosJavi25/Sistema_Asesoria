using Microsoft.EntityFrameworkCore;
using Sanalia.Core.Entities;
using Sanalia.Core.Interfaces;
using Sanalia.Infrastructure.Data.Context;

namespace Sanalia.Infrastructure.Repositories
{
    public class CotizacionRepository(SanaliaDbContext db) : Repository<Cotizacion>(db), ICotizacionRepository
    {
        public Task<Cotizacion?> GetConDetallesAsync(int idCotizacion, CancellationToken ct = default) =>
            Set.Include(c => c.Asesoria)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.Aseguradora)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.Id == idCotizacion, ct);
    }
}