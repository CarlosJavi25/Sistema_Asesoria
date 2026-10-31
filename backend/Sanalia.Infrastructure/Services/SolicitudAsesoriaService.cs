using Microsoft.EntityFrameworkCore;
using Sanalia.Core.Entities;
using Sanalia.Core.Enums;
using Sanalia.Core.Interfaces;
using Sanalia.Core.Interfaces.IServices;
using Sanalia.Infrastructure.Data.Context;

namespace Sanalia.Infrastructure.Services;

public sealed class SolicitudAsesoriaService(
    SanaliaDbContext db,
    IClienteRepository clientes,
    IAsesorRepository asesores,
    IRepository<TipoSeguro> tipos,
    IHorarioDisponibleRepository horarios,
    ISolicitudAsesoriaRepository solicitudes) : ISolicitudAsesoriaService
{
    public async Task<SolicitudAsesoria> CrearAsync(
        int idCliente,
        int idTipo,
        int idHorario,
        decimal? presupuesto = null,
        string? descripcionNecesidad = null,
        CancellationToken ct = default)
    {
        if (idCliente <= 0)
            throw new ArgumentOutOfRangeException(nameof(idCliente));
        if (idTipo <= 0)
            throw new ArgumentOutOfRangeException(nameof(idTipo));
        if (idHorario <= 0)
            throw new ArgumentOutOfRangeException(nameof(idHorario));
        if (presupuesto < 0)
            throw new ArgumentOutOfRangeException(nameof(presupuesto), "El presupuesto no puede ser negativo.");
        if (await clientes.GetByIdAsync(idCliente, ct) is null)
            throw new KeyNotFoundException($"No existe el cliente {idCliente}.");
        if (await tipos.GetByIdAsync(idTipo, ct) is null)
            throw new KeyNotFoundException($"No existe el tipo de seguro {idTipo}.");

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        if (!await horarios.TryReservarAsync(idHorario, ct))
            throw new InvalidOperationException("El horario ya no está disponible.");

        var horario = await horarios.GetByIdAsync(idHorario, ct);
        if (horario is null)
            throw new InvalidOperationException("El horario seleccionado no existe.");

        var ahora = DateTime.UtcNow;
        if (horario.Fecha < DateOnly.FromDateTime(ahora)
            || (horario.Fecha == DateOnly.FromDateTime(ahora)
                && horario.HoraInicio <= TimeOnly.FromDateTime(ahora)))
            throw new InvalidOperationException("El horario seleccionado ya pasó.");

        var asesor = await asesores.GetByIdAsync(horario.AsesorId, ct)
            ?? throw new InvalidOperationException("El horario seleccionado no tiene un asesor válido.");
        if (!asesor.Activo)
            throw new InvalidOperationException("El horario seleccionado pertenece a un asesor inactivo.");

        var solicitud = new SolicitudAsesoria
        {
            ClienteId = idCliente,
            TipoId = idTipo,
            HorarioId = idHorario,
            Presupuesto = presupuesto,
            DescripcionNecesidad = descripcionNecesidad?.Trim(),
            Estado = EstadoSolicitudAsesoria.Pendiente,
            FechaSolicitud = DateTime.UtcNow
        };

        await solicitudes.AddAsync(solicitud, ct);
        await solicitudes.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return solicitud;
    }

    public Task<IReadOnlyList<SolicitudAsesoria>> GetPendientesAsync(CancellationToken ct = default) =>
        solicitudes.GetPendientesAsync(ct);

    public Task<IReadOnlyList<SolicitudAsesoria>> GetOperativasAsync(CancellationToken ct = default) =>
        solicitudes.GetOperativasAsync(ct);

    public Task<IReadOnlyList<SolicitudAsesoria>> GetByClienteAsync(
        int idCliente,
        CancellationToken ct = default)
    {
        if (idCliente <= 0)
            throw new ArgumentOutOfRangeException(nameof(idCliente));

        return solicitudes.GetByClienteAsync(idCliente, ct);
    }

    public Task<SolicitudAsesoria?> GetByIdAsync(
        int idSolicitud,
        CancellationToken ct = default)
    {
        if (idSolicitud <= 0)
            throw new ArgumentOutOfRangeException(nameof(idSolicitud));

        return solicitudes.GetConDetalleAsync(idSolicitud, ct);
    }

    public async Task<SolicitudAsesoria> CambiarEstadoAsync(
        int idSolicitud,
        EstadoSolicitudAsesoria nuevoEstado,
        CancellationToken ct = default)
    {
        if (idSolicitud <= 0)
            throw new ArgumentOutOfRangeException(nameof(idSolicitud));

        var solicitud = await solicitudes.GetByIdAsync(idSolicitud, ct)
            ?? throw new KeyNotFoundException($"No existe la solicitud de asesoría {idSolicitud}.");
        if (!EsTransicionValida(solicitud.Estado, nuevoEstado))
            throw new InvalidOperationException(
                $"No se puede cambiar una solicitud de {solicitud.Estado} a {nuevoEstado}.");

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        if (nuevoEstado == EstadoSolicitudAsesoria.Cancelada && solicitud.HorarioId.HasValue)
            await horarios.LiberarAsync(solicitud.HorarioId.Value, ct);

        solicitud.Estado = nuevoEstado;
        solicitudes.Update(solicitud);
        await solicitudes.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return solicitud;
    }

    private static bool EsTransicionValida(EstadoSolicitudAsesoria actual, EstadoSolicitudAsesoria siguiente) =>
        (actual, siguiente) switch
        {
            (EstadoSolicitudAsesoria.Pendiente, EstadoSolicitudAsesoria.Confirmada) => true,
            (EstadoSolicitudAsesoria.Pendiente, EstadoSolicitudAsesoria.Cancelada) => true,
            (EstadoSolicitudAsesoria.Confirmada, EstadoSolicitudAsesoria.Atendida) => true,
            (EstadoSolicitudAsesoria.Confirmada, EstadoSolicitudAsesoria.Cancelada) => true,
            _ => false
        };
}
