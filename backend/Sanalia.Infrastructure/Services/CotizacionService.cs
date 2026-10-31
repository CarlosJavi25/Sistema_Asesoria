using Sanalia.Core.Entities;
using Sanalia.Core.Enums;
using Sanalia.Core.Interfaces;
using Sanalia.Core.Interfaces.IServices;

namespace Sanalia.Infrastructure.Services;

public sealed class CotizacionService(
    IRepository<Asesoria> asesorias,
    IProductoSeguroRepository productos,
    ICotizacionRepository cotizaciones,
    IProductoSeguroService comparador) : ICotizacionService
{
    public async Task<Cotizacion> CrearAsync(
        int idAsesoria,
        int idAsesor,
        IReadOnlyCollection<DetalleCotizacion> detalles,
        string? observaciones = null,
        CancellationToken ct = default)
    {
        if (idAsesoria <= 0)
            throw new ArgumentOutOfRangeException(nameof(idAsesoria));
        if (idAsesor <= 0)
            throw new ArgumentOutOfRangeException(nameof(idAsesor));
        ArgumentNullException.ThrowIfNull(detalles);
        if (detalles.Count == 0)
            throw new ArgumentException("La cotización debe incluir al menos un producto.", nameof(detalles));
        var asesoria = await asesorias.GetByIdAsync(idAsesoria, ct)
            ?? throw new KeyNotFoundException($"No existe la asesoría {idAsesoria}.");
        if (asesoria.AsesorId != idAsesor)
            throw new UnauthorizedAccessException("Solo el asesor asignado puede crear una cotización.");

        var comparacion = await comparador.CompararAsync(asesoria.SolicitudId, ct);
        var idsProducto = new HashSet<int>();
        var alternativas = new List<DetalleCotizacion>(detalles.Count);
        foreach (var detalle in detalles)
        {
            if (detalle.ProductoId <= 0)
                throw new ArgumentOutOfRangeException(nameof(detalles), "Todos los productos deben tener un identificador válido.");
            if (!idsProducto.Add(detalle.ProductoId))
                throw new ArgumentException("No se puede repetir un producto en la misma cotización.", nameof(detalles));
            if (detalle.PrimaCotizada < 0)
                throw new ArgumentOutOfRangeException(nameof(detalles), "La prima cotizada no puede ser negativa.");
            if (detalle.PuntajeCoincidencia is < 0 or > 100)
                throw new ArgumentOutOfRangeException(nameof(detalles), "El puntaje debe estar entre 0 y 100.");

            var producto = await productos.GetConDetalleAsync(detalle.ProductoId, ct)
                ?? throw new KeyNotFoundException($"No existe el producto {detalle.ProductoId}.");
            if (!producto.Vigente)
                throw new InvalidOperationException($"El producto {producto.Id} no está vigente.");
            var alternativaComparada = comparacion.FirstOrDefault(d => d.ProductoId == producto.Id)
                ?? throw new InvalidOperationException(
                    $"El producto {producto.Id} no es una alternativa vigente para la solicitud.");

            alternativas.Add(new DetalleCotizacion
            {
                ProductoId = producto.Id,
                Producto = producto,
                PrimaCotizada = detalle.PrimaCotizada,
                PuntajeCoincidencia = alternativaComparada.PuntajeCoincidencia
            });
        }

        alternativas.OrderByDescending(d => d.PuntajeCoincidencia ?? 0m)
            .ThenBy(d => d.PrimaCotizada)
            .First()
            .Recomendada = true;

        var cotizacion = new Cotizacion
        {
            AsesoriaId = idAsesoria,
            FechaEmision = DateOnly.FromDateTime(DateTime.UtcNow),
            Estado = EstadoCotizacion.Borrador,
            Observaciones = observaciones?.Trim(),
            Detalles = alternativas
        };

        await cotizaciones.AddAsync(cotizacion, ct);
        await cotizaciones.SaveChangesAsync(ct);
        return cotizacion;
    }

    public Task<Cotizacion?> GetConDetallesAsync(int idCotizacion, CancellationToken ct = default)
    {
        if (idCotizacion <= 0)
            throw new ArgumentOutOfRangeException(nameof(idCotizacion));

        return cotizaciones.GetConDetallesAsync(idCotizacion, ct);
    }

    public Task<IReadOnlyList<Cotizacion>> GetByAsesorAsync(int? idAsesor, CancellationToken ct = default)
    {
        if (idAsesor is <= 0)
            throw new ArgumentOutOfRangeException(nameof(idAsesor));

        return cotizaciones.GetByAsesorAsync(idAsesor, ct);
    }

    public async Task<Cotizacion> CambiarEstadoAsync(
        int idCotizacion,
        EstadoCotizacion nuevoEstado,
        CancellationToken ct = default)
    {
        if (idCotizacion <= 0)
            throw new ArgumentOutOfRangeException(nameof(idCotizacion));

        var cotizacion = await cotizaciones.GetByIdAsync(idCotizacion, ct)
            ?? throw new KeyNotFoundException($"No existe la cotización {idCotizacion}.");

        var transicionValida = (cotizacion.Estado, nuevoEstado) switch
        {
            (EstadoCotizacion.Borrador, EstadoCotizacion.Presentada) => true,
            (EstadoCotizacion.Presentada, EstadoCotizacion.Aceptada) => true,
            (EstadoCotizacion.Presentada, EstadoCotizacion.Rechazada) => true,
            _ => false
        };
        if (!transicionValida)
            throw new InvalidOperationException(
                $"No se puede cambiar una cotización de {cotizacion.Estado} a {nuevoEstado}.");

        cotizacion.Estado = nuevoEstado;
        cotizaciones.Update(cotizacion);
        await cotizaciones.SaveChangesAsync(ct);
        return cotizacion;
    }
}
