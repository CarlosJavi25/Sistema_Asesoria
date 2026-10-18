using Sanalia.Core.Entities;
using Sanalia.Core.Enums;

namespace Sanalia.Core.Interfaces.IServices;

public interface ICotizacionService
{
    Task<Cotizacion> CrearAsync(
        int idAsesoria,
        IReadOnlyCollection<DetalleCotizacion> detalles,
        string? observaciones = null,
        CancellationToken ct = default);

    Task<Cotizacion?> GetConDetallesAsync(int idCotizacion, CancellationToken ct = default);
    Task<Cotizacion> CambiarEstadoAsync(
        int idCotizacion,
        EstadoCotizacion nuevoEstado,
        CancellationToken ct = default);
}
