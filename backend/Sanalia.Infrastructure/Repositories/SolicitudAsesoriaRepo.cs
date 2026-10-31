using Microsoft.EntityFrameworkCore;
using Sanalia.Core.Entities;
using Sanalia.Core.Enums;
using Sanalia.Core.Interfaces;
using Sanalia.Infrastructure.Data.Context;

namespace Sanalia.Infrastructure.Repositories;

public class SolicitudAsesoriaRepository(SanaliaDbContext db) : Repository<SolicitudAsesoria>(db), ISolicitudAsesoriaRepository
{
    public async Task<IReadOnlyList<SolicitudAsesoria>> GetPendientesAsync(CancellationToken ct = default) =>
        await Set.AsNoTracking()
            .Include(s => s.Cliente)
            .Include(s => s.TipoSeguro)
            .Include(s => s.Horario)
                .ThenInclude(h => h!.Asesor)
            .Where(s => s.Estado == EstadoSolicitudAsesoria.Pendiente)
            .OrderBy(s => s.FechaSolicitud)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SolicitudAsesoria>> GetOperativasAsync(CancellationToken ct = default) =>
        await Set.AsNoTracking()
            .Include(s => s.Cliente)
            .Include(s => s.TipoSeguro)
            .Include(s => s.Horario)
                .ThenInclude(h => h!.Asesor)
            .Where(s => s.Estado == EstadoSolicitudAsesoria.Pendiente
                     || s.Estado == EstadoSolicitudAsesoria.Confirmada)
            .OrderBy(s => s.FechaSolicitud)
            .ToListAsync(ct);

    public Task<SolicitudAsesoria?> GetConDetalleAsync(int idSolicitud, CancellationToken ct = default) =>
        Set.AsNoTracking()
            .Include(s => s.Cliente)
            .Include(s => s.TipoSeguro)
            .Include(s => s.Horario)
                .ThenInclude(h => h!.Asesor)
            .FirstOrDefaultAsync(s => s.Id == idSolicitud, ct);

    public async Task<IReadOnlyList<SolicitudAsesoria>> GetByClienteAsync(int idCliente, CancellationToken ct = default) =>
        await Set.AsNoTracking()
            .Include(s => s.TipoSeguro)
            .Include(s => s.Horario)
                .ThenInclude(h => h!.Asesor)
            .Where(s => s.ClienteId == idCliente)
            .OrderByDescending(s => s.FechaSolicitud)
            .ToListAsync(ct);
}
