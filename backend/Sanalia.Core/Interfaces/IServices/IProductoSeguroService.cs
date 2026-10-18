using Sanalia.Core.Entities;

namespace Sanalia.Core.Interfaces.IServices;

public interface IProductoSeguroService
{
    Task<IReadOnlyList<TipoSeguro>> GetTiposSeguroAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Aseguradora>> GetAseguradorasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ProductoSeguro>> GetVigentesPorTipoAsync(int idTipo, CancellationToken ct = default);
    Task<ProductoSeguro?> GetConDetalleAsync(int idProducto, CancellationToken ct = default);

    /// <summary>
    /// Puntúa alternativas sobre 100: prima frente al presupuesto (30), nivel de cobertura (25),
    /// deducible relativo (20), presencia de beneficios adicionales (15) y límites agregados (10).
    /// Devuelve las alternativas ordenadas por puntaje y marca la primera como recomendada.
    /// </summary>
    Task<IReadOnlyList<DetalleCotizacion>> CompararAsync(int idSolicitudAsesoria, CancellationToken ct = default);
}
