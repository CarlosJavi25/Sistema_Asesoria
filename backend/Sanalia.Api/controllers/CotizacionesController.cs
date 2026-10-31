using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sanalia.Api.Contracts;
using Sanalia.Api.Mappings;
using Sanalia.Api.Security;
using Sanalia.Core.Entities;
using Sanalia.Core.Enums;
using Sanalia.Core.Interfaces.IServices;

namespace Sanalia.Api.Controllers;

[ApiController]
[Route("api")]
[Produces("application/json")]
public sealed class CotizacionesController(ICotizacionService cotizaciones) : ControllerBase
{
    [Authorize(Roles = JwtRoles.Personal)]
    [HttpPost("asesorias/{asesoriaId:int}/cotizaciones")]
    [ProducesResponseType<CotizacionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CotizacionResponse>> Create(
        int asesoriaId,
        CreateCotizacionRequest request,
        CancellationToken cancellationToken)
    {
        var details = request.Detalles.Select(d => new DetalleCotizacion
        {
            ProductoId = d.ProductoId,
            PrimaCotizada = d.PrimaCotizada
        }).ToArray();
        var cotizacion = await cotizaciones.CrearAsync(
            asesoriaId,
            GetUserId(),
            details,
            request.Observaciones,
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = cotizacion.Id }, cotizacion.ToResponse());
    }

    [Authorize]
    [HttpGet("cotizaciones/{id:int}")]
    [ProducesResponseType<CotizacionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CotizacionResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var cotizacion = await GetAuthorizedCotizacion(id, cancellationToken);
        return Ok(cotizacion.ToResponse());
    }

    [Authorize]
    [HttpPatch("cotizaciones/{id:int}/estado")]
    [ProducesResponseType<CotizacionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CotizacionResponse>> ChangeState(
        int id,
        ChangeCotizacionStateRequest request,
        CancellationToken cancellationToken)
    {
        _ = await GetAuthorizedCotizacion(id, cancellationToken);
        if (User.IsInRole(JwtRoles.Cliente)
            && request.Estado is not (EstadoCotizacion.Aceptada or EstadoCotizacion.Rechazada))
        {
            throw new UnauthorizedAccessException("Los clientes solo pueden aceptar o rechazar cotizaciones.");
        }

        _ = await cotizaciones.CambiarEstadoAsync(id, request.Estado, cancellationToken);
        var updated = await cotizaciones.GetConDetallesAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe la cotización {id}.");
        return Ok(updated.ToResponse());
    }

    private async Task<Cotizacion> GetAuthorizedCotizacion(
        int id,
        CancellationToken cancellationToken)
    {
        var cotizacion = await cotizaciones.GetConDetallesAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe la cotización {id}.");

        if (User.IsInRole(JwtRoles.Administrador))
            return cotizacion;
        if (User.IsInRole(JwtRoles.Cliente) && cotizacion.Asesoria.ClienteId == GetUserId())
            return cotizacion;
        if (User.IsInRole(JwtRoles.Asesor) && cotizacion.Asesoria.AsesorId == GetUserId())
            return cotizacion;

        throw new UnauthorizedAccessException();
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            System.Globalization.CultureInfo.InvariantCulture);
}
