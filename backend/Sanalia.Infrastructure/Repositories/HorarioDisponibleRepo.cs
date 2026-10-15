using Microsoft.EntityFrameworkCore;
using Sanalia.Core.Entities;
using Sanalia.Core.Interfaces;
using Sanalia.Infrastructure.Data.Context;

namespace Sanalia.Infrastructure.Repositories
{
    public class HorarioDisponibleRepository(SanaliaDbContext db) : Repository<HorarioDisponible>(db), IHorarioDisponibleRepository
    {
        public async Task<IReadOnlyList<HorarioDisponible>> GetDisponiblesAsync(
            DateOnly desde, int? idAsesor = null, CancellationToken ct = default)
        {
            var query = Set.AsNoTracking()
                .Include(h => h.Asesor)
                .Where(h => h.Disponible && h.Fecha >= desde);

            if (idAsesor.HasValue)
                query = query.Where(h => h.AsesorId == idAsesor.Value);

            return await query
                .OrderBy(h => h.Fecha)
                .ThenBy(h => h.HoraInicio)
                .ToListAsync(ct);
        }

        public async Task<bool> TryReservarAsync(int idHorario, CancellationToken ct = default)
        {
            // UPDATE ... WHERE id = @id AND disponible = 1 : si dos clientes llegan a la vez,
            // solo uno modifica la fila (filas afectadas = 1) y el otro recibe false.
            var filas = await Set
                .Where(h => h.Id == idHorario && h.Disponible)
                .ExecuteUpdateAsync(s => s.SetProperty(h => h.Disponible, false), ct);

            return filas == 1;
        }

        public async Task LiberarAsync(int idHorario, CancellationToken ct = default) =>
            await Set
                .Where(h => h.Id == idHorario)
                .ExecuteUpdateAsync(s => s.SetProperty(h => h.Disponible, true), ct);
    }
}