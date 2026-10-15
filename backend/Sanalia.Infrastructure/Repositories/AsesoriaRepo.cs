using Microsoft.EntityFrameworkCore;
using Sanalia.Core.Entities;
using Sanalia.Core.Interfaces;
using Sanalia.Infrastructure.Data.Context;

namespace Sanalia.Infrastructure.Repositories
{
    public class AsesoriaRepository(SanaliaDbContext db) : Repository<Asesoria>(db), IAsesoriaRepository
    {
        public async Task<IReadOnlyList<Asesoria>> GetHistorialPorClienteAsync(int idCliente, CancellationToken ct = default) =>
            await Set.AsNoTracking()
                .Include(a => a.Asesor)
                .Include(a => a.Cotizaciones)
                    .ThenInclude(c => c.Detalles)
                        .ThenInclude(d => d.Producto)
                            .ThenInclude(p => p.Aseguradora)
                .Where(a => a.ClienteId == idCliente)
                .OrderByDescending(a => a.FechaAsesoria)
                .AsSplitQuery()
                .ToListAsync(ct);
    }
}