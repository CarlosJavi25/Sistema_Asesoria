using Sanalia.Core.Entities;
using Sanalia.Core.Interfaces;
using Sanalia.Core.Interfaces.IServices;

namespace Sanalia.Infrastructure.Services;

public sealed class ProductoSeguroService(
    IProductoSeguroRepository productos,
    IRepository<SolicitudAsesoria> solicitudes,
    IRepository<TipoSeguro> tipos,
    IRepository<Aseguradora> aseguradoras) : IProductoSeguroService
{
    public Task<IReadOnlyList<TipoSeguro>> GetTiposSeguroAsync(CancellationToken ct = default) =>
        tipos.GetAllAsync(ct);

    public Task<IReadOnlyList<Aseguradora>> GetAseguradorasAsync(CancellationToken ct = default) =>
        aseguradoras.GetAllAsync(ct);

    public async Task<IReadOnlyList<ProductoSeguro>> GetVigentesPorTipoAsync(
        int idTipo,
        CancellationToken ct = default)
    {
        if (idTipo <= 0)
            throw new ArgumentOutOfRangeException(nameof(idTipo));
        if (await tipos.GetByIdAsync(idTipo, ct) is null)
            throw new KeyNotFoundException($"No existe el tipo de seguro {idTipo}.");

        return await productos.GetVigentesPorTipoAsync(idTipo, ct);
    }

    public Task<ProductoSeguro?> GetConDetalleAsync(int idProducto, CancellationToken ct = default)
    {
        if (idProducto <= 0)
            throw new ArgumentOutOfRangeException(nameof(idProducto));

        return productos.GetConDetalleAsync(idProducto, ct);
    }

    public async Task<IReadOnlyList<DetalleCotizacion>> CompararAsync(
        int idSolicitudAsesoria,
        CancellationToken ct = default)
    {
        if (idSolicitudAsesoria <= 0)
            throw new ArgumentOutOfRangeException(nameof(idSolicitudAsesoria));

        var solicitud = await solicitudes.GetByIdAsync(idSolicitudAsesoria, ct)
            ?? throw new KeyNotFoundException($"No existe la solicitud de asesoría {idSolicitudAsesoria}.");

        var candidatos = await GetVigentesPorTipoAsync(solicitud.TipoId, ct);
        if (candidatos.Count == 0)
            return Array.Empty<DetalleCotizacion>();

        var deducibleMinimo = candidatos.Min(p => p.Deducible);
        var deducibleMaximo = candidatos.Max(p => p.Deducible);
        var limiteMaximo = candidatos.Max(SumarLimites);

        var detalles = candidatos.Select(producto =>
        {
            var puntajePrima = PuntuarPrima(producto.Prima, solicitud.Presupuesto);
            var puntajeCobertura = producto.NivelCobertura switch
            {
                Sanalia.Core.Enums.NivelCobertura.Basico => 25m / 3m,
                Sanalia.Core.Enums.NivelCobertura.Intermedio => 50m / 3m,
                Sanalia.Core.Enums.NivelCobertura.Completo => 25m,
                _ => throw new InvalidOperationException(
                    $"Nivel de cobertura desconocido: {producto.NivelCobertura}.")
            };
            var puntajeDeducible = deducibleMaximo == deducibleMinimo
                ? 20m
                : 20m * (deducibleMaximo - producto.Deducible) / (deducibleMaximo - deducibleMinimo);
            var puntajeBeneficios = string.IsNullOrWhiteSpace(producto.BeneficiosAdicionales) ? 0m : 15m;
            var limite = SumarLimites(producto);
            var puntajeLimites = limiteMaximo == 0m ? 0m : 10m * limite / limiteMaximo;

            return new DetalleCotizacion
            {
                ProductoId = producto.Id,
                Producto = producto,
                PrimaCotizada = producto.Prima,
                PuntajeCoincidencia = Math.Round(
                    puntajePrima + puntajeCobertura + puntajeDeducible + puntajeBeneficios + puntajeLimites,
                    2,
                    MidpointRounding.AwayFromZero)
            };
        }).OrderByDescending(d => d.PuntajeCoincidencia)
          .ThenBy(d => d.PrimaCotizada)
          .ToList();

        detalles[0].Recomendada = true;
        return detalles;
    }

    private static decimal PuntuarPrima(decimal prima, decimal? presupuesto)
    {
        if (prima < 0)
            throw new InvalidOperationException("Un producto tiene una prima negativa.");
        if (!presupuesto.HasValue)
            return 0m;
        if (prima <= presupuesto.Value)
            return 30m;
        if (prima == 0m)
            return 30m;

        return 30m * presupuesto.Value / prima;
    }

    private static decimal SumarLimites(ProductoSeguro producto) =>
        producto.Coberturas.Sum(c => c.Limite ?? 0m);
}
