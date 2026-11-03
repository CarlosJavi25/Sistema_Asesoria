using Sanalia.Core.Entities;
using Sanalia.Core.Enums;
using Sanalia.Core.Interfaces;
using Sanalia.Core.Interfaces.IServices;

namespace Sanalia.Infrastructure.Services;

public sealed class SolicitudAsistenciaService(
    IClienteRepository clientes,
    IAsesorRepository asesores,
    ISolicitudAsistenciaRepository solicitudes) : ISolicitudAsistenciaService
{
    public async Task<SolicitudAsistencia> CrearAsync(
        int idCliente,
        string motivo,
        CancellationToken ct = default)
    {
        if (idCliente <= 0)
            throw new ArgumentOutOfRangeException(nameof(idCliente));
        ArgumentException.ThrowIfNullOrWhiteSpace(motivo);
        if (await clientes.GetByIdAsync(idCliente, ct) is null)
            throw new KeyNotFoundException($"No existe el cliente {idCliente}.");

        var solicitud = new SolicitudAsistencia
        {
            ClienteId = idCliente,
            Motivo = motivo.Trim(),
            Estado = EstadoAsistencia.Pendiente,
            FechaSolicitud = DateTime.UtcNow
        };

        await solicitudes.AddAsync(solicitud, ct);
        await solicitudes.SaveChangesAsync(ct);
        return solicitud;
    }

    public Task<IReadOnlyList<SolicitudAsistencia>> GetAbiertasAsync(CancellationToken ct = default) =>
        solicitudes.GetAbiertasAsync(ct);

    public Task<IReadOnlyList<SolicitudAsistencia>> GetTodasAsync(CancellationToken ct = default) =>
        solicitudes.GetTodasAsync(ct);

    public Task<IReadOnlyList<SolicitudAsistencia>> GetByClienteAsync(
        int idCliente,
        CancellationToken ct = default)
    {
        if (idCliente <= 0)
            throw new ArgumentOutOfRangeException(nameof(idCliente));

        return solicitudes.FindAsync(s => s.ClienteId == idCliente, ct);
    }

    public async Task<SolicitudAsistencia> AsignarAsync(
        int idSolicitud,
        int idAsesor,
        CancellationToken ct = default)
    {
        if (idSolicitud <= 0)
            throw new ArgumentOutOfRangeException(nameof(idSolicitud));
        if (idAsesor <= 0)
            throw new ArgumentOutOfRangeException(nameof(idAsesor));

        var asesor = await asesores.GetByIdAsync(idAsesor, ct)
            ?? throw new KeyNotFoundException($"No existe el asesor {idAsesor}.");
        if (!asesor.Activo)
            throw new InvalidOperationException("No se puede asignar la solicitud a un asesor inactivo.");

        var solicitud = await solicitudes.GetByIdAsync(idSolicitud, ct)
            ?? throw new KeyNotFoundException($"No existe la solicitud de asistencia {idSolicitud}.");
        if (solicitud.Estado == EstadoAsistencia.Resuelta)
            throw new InvalidOperationException("No se puede asignar una solicitud ya resuelta.");
        if (solicitud.AsesorId.HasValue && solicitud.AsesorId != idAsesor)
            throw new InvalidOperationException("La solicitud ya está asignada a otro asesor.");

        solicitud.AsesorId = idAsesor;
        solicitud.Estado = EstadoAsistencia.EnProceso;
        solicitudes.Update(solicitud);
        await solicitudes.SaveChangesAsync(ct);
        return solicitud;
    }

    public async Task<SolicitudAsistencia> CambiarEstadoAsync(
        int idSolicitud,
        int idAsesor,
        EstadoAsistencia nuevoEstado,
        CancellationToken ct = default)
    {
        if (idSolicitud <= 0)
            throw new ArgumentOutOfRangeException(nameof(idSolicitud));
        if (idAsesor <= 0)
            throw new ArgumentOutOfRangeException(nameof(idAsesor));

        var solicitud = await solicitudes.GetByIdAsync(idSolicitud, ct)
            ?? throw new KeyNotFoundException($"No existe la solicitud de asistencia {idSolicitud}.");
        if (solicitud.AsesorId != idAsesor)
            throw new InvalidOperationException("Solo el asesor asignado puede actualizar la solicitud.");

        var transicionValida = solicitud.Estado == EstadoAsistencia.EnProceso
            && nuevoEstado == EstadoAsistencia.Resuelta;
        if (!transicionValida)
            throw new InvalidOperationException(
                $"No se puede cambiar una solicitud de {solicitud.Estado} a {nuevoEstado}.");

        solicitud.Estado = nuevoEstado;
        solicitudes.Update(solicitud);
        await solicitudes.SaveChangesAsync(ct);
        return solicitud;
    }
}
