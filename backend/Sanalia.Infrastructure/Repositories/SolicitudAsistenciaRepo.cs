using Microsoft.EntityFrameworkCore;
using Sanalia.Core.Entities;
using Sanalia.Core.Enums;
using Sanalia.Core.Interfaces;
using Sanalia.Infrastructure.Data.Context;

namespace Sanalia.Infrastructure.Repositories
{
    public class SolicitudAsistenciaRepository(SanaliaDbContext db) : Repository<SolicitudAsistencia>(db), ISolicitudAsistenciaRepository
    {
        public async Task<IReadOnlyList<SolicitudAsistencia>> GetAbiertasAsync(CancellationToken ct = default) =>
            await Set.AsNoTracking()
                .Include(s => s.Cliente)
                .Include(s => s.Asesor)
                .Where(s => s.Estado != EstadoAsistencia.Resuelta)
                .OrderBy(s => s.FechaSolicitud)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<SolicitudAsistencia>> GetTodasAsync(CancellationToken ct = default) =>
            await Set.AsNoTracking()
                .Include(s => s.Cliente)
                .Include(s => s.Asesor)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync(ct);
    }
}