using Sanalia.Core.Entities;
using Sanalia.Core.Enums;

namespace Sanalia.Core.Interfaces.IServices;

public interface ISolicitudAsesoriaService
{
    Task<SolicitudAsesoria> CrearAsync(
        int idCliente,
        int idTipo,
        int idHorario,
        decimal? presupuesto = null,
        string? descripcionNecesidad = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<SolicitudAsesoria>> GetPendientesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SolicitudAsesoria>> GetByClienteAsync(int idCliente, CancellationToken ct = default);
    Task<SolicitudAsesoria> CambiarEstadoAsync(
        int idSolicitud,
        EstadoSolicitudAsesoria nuevoEstado,
        CancellationToken ct = default);
}
