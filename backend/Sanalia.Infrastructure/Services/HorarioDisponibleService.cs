using Sanalia.Core.Entities;
using Sanalia.Core.Interfaces;
using Sanalia.Core.Interfaces.IServices;

namespace Sanalia.Infrastructure.Services;

public sealed class HorarioDisponibleService(
    IHorarioDisponibleRepository horarios,
    IAsesorRepository asesores) : IHorarioDisponibleService
{
    public Task<IReadOnlyList<HorarioDisponible>> GetDisponiblesAsync(
        DateOnly desde,
        int? idAsesor = null,
        CancellationToken ct = default)
    {
        if (idAsesor <= 0)
            throw new ArgumentOutOfRangeException(nameof(idAsesor));

        return horarios.GetDisponiblesAsync(desde, idAsesor, ct);
    }

    public async Task<HorarioDisponible> CrearAsync(
        int idAsesor,
        DateOnly fecha,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        CancellationToken ct = default)
    {
        var ahora = DateTime.UtcNow;
        if (idAsesor <= 0)
            throw new ArgumentOutOfRangeException(nameof(idAsesor));
        if (fecha < DateOnly.FromDateTime(ahora)
            || (fecha == DateOnly.FromDateTime(ahora) && horaInicio <= TimeOnly.FromDateTime(ahora)))
            throw new ArgumentException("El horario debe comenzar en el futuro.", nameof(fecha));
        if (horaInicio >= horaFin)
            throw new ArgumentException("La hora de inicio debe ser anterior a la hora de fin.");

        var asesor = await asesores.GetByIdAsync(idAsesor, ct)
            ?? throw new KeyNotFoundException($"No existe el asesor {idAsesor}.");
        if (!asesor.Activo)
            throw new InvalidOperationException("No se puede crear disponibilidad para un asesor inactivo.");

        var horario = new HorarioDisponible
        {
            AsesorId = idAsesor,
            Fecha = fecha,
            HoraInicio = horaInicio,
            HoraFin = horaFin,
            Disponible = true
        };

        await horarios.AddAsync(horario, ct);
        await horarios.SaveChangesAsync(ct);
        return horario;
    }
}
