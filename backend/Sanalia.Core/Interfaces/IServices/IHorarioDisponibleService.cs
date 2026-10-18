using Sanalia.Core.Entities;

namespace Sanalia.Core.Interfaces.IServices;

public interface IHorarioDisponibleService
{
    Task<IReadOnlyList<HorarioDisponible>> GetDisponiblesAsync(
        DateOnly desde,
        int? idAsesor = null,
        CancellationToken ct = default);

    Task<HorarioDisponible> CrearAsync(
        int idAsesor,
        DateOnly fecha,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        CancellationToken ct = default);
}
