using Microsoft.EntityFrameworkCore;
using Sanalia.Core.Entities;
using Sanalia.Core.Enums;
using Sanalia.Core.Interfaces;
using Sanalia.Core.Interfaces.IServices;
using Sanalia.Infrastructure.Data.Context;

namespace Sanalia.Infrastructure.Services;

public sealed class AsesoriaService(
    SanaliaDbContext db,
    ISolicitudAsesoriaRepository solicitudes,
    IAsesorRepository asesores,
    IAsesoriaRepository asesorias,
    IHorarioDisponibleRepository horarios) : IAsesoriaService
{
    public async Task<Asesoria> CrearAsync(
        int idSolicitud,
        int idAsesor,
        DateTime fechaAsesoria,
        string? necesidadesIdentificadas = null,
        string? notas = null,
        CancellationToken ct = default)
    {
        if (idSolicitud <= 0)
            throw new ArgumentOutOfRangeException(nameof(idSolicitud));
        if (idAsesor <= 0)
            throw new ArgumentOutOfRangeException(nameof(idAsesor));
        if (fechaAsesoria == default)
            throw new ArgumentException("La fecha de asesoría es obligatoria.", nameof(fechaAsesoria));
        if (fechaAsesoria > DateTime.UtcNow)
            throw new ArgumentException("No se puede registrar una asesoría con fecha futura.", nameof(fechaAsesoria));

        var solicitud = await solicitudes.GetByIdAsync(idSolicitud, ct)
            ?? throw new KeyNotFoundException($"No existe la solicitud de asesoría {idSolicitud}.");
        if (solicitud.Estado != EstadoSolicitudAsesoria.Confirmada)
            throw new InvalidOperationException("Solo se pueden registrar asesorías de solicitudes confirmadas.");
        var horario = solicitud.HorarioId.HasValue
            ? await horarios.GetByIdAsync(solicitud.HorarioId.Value, ct)
            : null;
        if (horario is null || horario.AsesorId != idAsesor)
            throw new UnauthorizedAccessException(
                "Solo el asesor asignado al horario reservado puede registrar la asesoría.");
        var citaFinaliza = new DateTimeOffset(
            horario.Fecha.ToDateTime(horario.HoraFin),
            TimeSpan.FromHours(-4));
        if (citaFinaliza > DateTimeOffset.UtcNow)
            throw new InvalidOperationException("No se puede registrar una asesoría antes de finalizar la cita.");

        var asesor = await asesores.GetByIdAsync(idAsesor, ct)
            ?? throw new KeyNotFoundException($"No existe el asesor {idAsesor}.");
        if (!asesor.Activo)
            throw new InvalidOperationException("No se puede asignar una asesoría a un asesor inactivo.");
        if ((await asesorias.FindAsync(a => a.SolicitudId == idSolicitud, ct)).Count > 0)
            throw new InvalidOperationException("La solicitud ya tiene una asesoría registrada.");

        var asesoria = new Asesoria
        {
            SolicitudId = idSolicitud,
            ClienteId = solicitud.ClienteId,
            AsesorId = idAsesor,
            FechaAsesoria = fechaAsesoria,
            NecesidadesIdentificadas = necesidadesIdentificadas?.Trim(),
            Notas = notas?.Trim()
        };

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        solicitud.Estado = EstadoSolicitudAsesoria.Atendida;
        solicitudes.Update(solicitud);
        await asesorias.AddAsync(asesoria, ct);
        await asesorias.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return asesoria;
    }

    public Task<IReadOnlyList<Asesoria>> GetHistorialPorClienteAsync(
        int idCliente,
        CancellationToken ct = default)
    {
        if (idCliente <= 0)
            throw new ArgumentOutOfRangeException(nameof(idCliente));

        return asesorias.GetHistorialPorClienteAsync(idCliente, ct);
    }

    public Task<Asesoria?> GetBySolicitudAsync(int idSolicitud, CancellationToken ct = default)
    {
        if (idSolicitud <= 0)
            throw new ArgumentOutOfRangeException(nameof(idSolicitud));

        return asesorias.GetBySolicitudAsync(idSolicitud, ct);
    }

    public Task<Asesoria?> GetByIdAsync(int idAsesoria, CancellationToken ct = default)
    {
        if (idAsesoria <= 0)
            throw new ArgumentOutOfRangeException(nameof(idAsesoria));

        return asesorias.GetByIdAsync(idAsesoria, ct);
    }

    public Task<IReadOnlyList<Asesoria>> GetByAsesorAsync(int? idAsesor, CancellationToken ct = default)
    {
        if (idAsesor is <= 0)
            throw new ArgumentOutOfRangeException(nameof(idAsesor));

        return asesorias.GetByAsesorAsync(idAsesor, ct);
    }
}
