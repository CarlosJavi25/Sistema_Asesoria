using Sanalia.Core.Entities;
using Sanalia.Core.Enums;

namespace Sanalia.Core.Interfaces.IServices;

public interface ISolicitudAsistenciaService
{
    Task<SolicitudAsistencia> CrearAsync(int idCliente, string motivo, CancellationToken ct = default);
    Task<IReadOnlyList<SolicitudAsistencia>> GetAbiertasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SolicitudAsistencia>> GetTodasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SolicitudAsistencia>> GetByClienteAsync(int idCliente, CancellationToken ct = default);
    Task<SolicitudAsistencia> AsignarAsync(int idSolicitud, int idAsesor, CancellationToken ct = default);
    Task<SolicitudAsistencia> CambiarEstadoAsync(
        int idSolicitud,
        int idAsesor,
        EstadoAsistencia nuevoEstado,
        CancellationToken ct = default);
}
