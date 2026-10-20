using Microsoft.AspNetCore.Mvc;
using Sanalia.Api.Contracts;
using Sanalia.Api.Mappings;
using Sanalia.Core.Interfaces.IServices;

namespace Sanalia.Api.Controllers;

[ApiController]
[Route("api/catalog")]
[Produces("application/json")]
public sealed class CatalogController(IProductoSeguroService productos) : ControllerBase
{
    [HttpGet("tipos-seguro")]
    [ProducesResponseType<IReadOnlyList<TipoSeguroResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TipoSeguroResponse>>> GetTipos(
        CancellationToken cancellationToken) =>
        Ok((await productos.GetTiposSeguroAsync(cancellationToken)).Select(t => t.ToResponse()).ToArray());

    [HttpGet("aseguradoras")]
    [ProducesResponseType<IReadOnlyList<AseguradoraResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AseguradoraResponse>>> GetAseguradoras(
        CancellationToken cancellationToken) =>
        Ok((await productos.GetAseguradorasAsync(cancellationToken)).Select(a => a.ToResponse()).ToArray());

    [HttpGet("tipos-seguro/{tipoId:int}/productos")]
    [ProducesResponseType<IReadOnlyList<ProductoSeguroResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProductoSeguroResponse>>> GetProductos(
        int tipoId,
        CancellationToken cancellationToken) =>
        Ok((await productos.GetVigentesPorTipoAsync(tipoId, cancellationToken))
            .Select(p => p.ToResponse()).ToArray());

    [HttpGet("productos/{id:int}")]
    [ProducesResponseType<ProductoSeguroResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoSeguroResponse>> GetProducto(
        int id,
        CancellationToken cancellationToken)
    {
        var producto = await productos.GetConDetalleAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el producto {id}.");
        return Ok(producto.ToResponse());
    }
}
