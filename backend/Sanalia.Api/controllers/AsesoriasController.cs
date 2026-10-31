using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sanalia.Api.Contracts;
using Sanalia.Api.Mappings;
using Sanalia.Api.Security;
using Sanalia.Core.Interfaces.IServices;

namespace Sanalia.Api.Controllers;

[ApiController]
[Route("api/asesorias")]
[Produces("application/json")]
public sealed class AsesoriasController(
    IAsesoriaService asesorias,
    ISolicitudAsesoriaService solicitudes) : ControllerBase
{
    [Authorize(Roles = JwtRoles.Personal)]
    [HttpPost]
    [ProducesResponseType<AsesoriaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AsesoriaResponse>> Create(
        CreateAsesoriaRequest request,
        CancellationToken cancellationToken)
    {
        var advisorId = GetUserId();
        if (User.IsInRole(JwtRoles.Administrador))
        {
            var solicitud = await solicitudes.GetByIdAsync(request.SolicitudId, cancellationToken)
                ?? throw new KeyNotFoundException($"No existe la solicitud de asesoría {request.SolicitudId}.");
            advisorId = solicitud.Horario?.AsesorId
                ?? throw new InvalidOperationException("La solicitud no tiene un asesor asignado.");
        }

        var asesoria = await asesorias.CrearAsync(
            request.SolicitudId,
            advisorId,
            request.FechaAsesoria.UtcDateTime,
            request.NecesidadesIdentificadas,
            request.Notas,
            cancellationToken);
        var response = asesoria.ToResponse();
        return CreatedAtAction(
            nameof(GetBySolicitud),
            new { solicitudId = response.SolicitudId },
            response);
    }

    [Authorize(Roles = JwtRoles.Personal)]
    [HttpGet("personal")]
    [ProducesResponseType<IReadOnlyList<AsesoriaSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AsesoriaSummaryResponse>>> GetForStaff(
        CancellationToken cancellationToken)
    {
        int? advisorId = User.IsInRole(JwtRoles.Administrador) ? null : GetUserId();
        var results = await asesorias.GetByAsesorAsync(advisorId, cancellationToken);
        return Ok(results.Select(item => item.ToSummaryResponse()).ToArray());
    }

    [Authorize(Roles = JwtRoles.Personal)]
    [HttpGet("solicitud/{solicitudId:int}")]
    [ProducesResponseType<AsesoriaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AsesoriaResponse>> GetBySolicitud(
        int solicitudId,
        CancellationToken cancellationToken)
    {
        var asesoria = await asesorias.GetBySolicitudAsync(solicitudId, cancellationToken)
            ?? throw new KeyNotFoundException($"La solicitud {solicitudId} todavía no tiene una asesoría registrada.");

        if (!User.IsInRole(JwtRoles.Administrador) && asesoria.AsesorId != GetUserId())
            throw new UnauthorizedAccessException();

        return Ok(asesoria.ToResponse());
    }

    [Authorize(Roles = JwtRoles.Cliente)]
    [HttpGet("mias")]
    [ProducesResponseType<IReadOnlyList<AsesoriaResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AsesoriaResponse>>> GetMine(
        CancellationToken cancellationToken) =>
        Ok((await asesorias.GetHistorialPorClienteAsync(GetUserId(), cancellationToken))
            .Select(a => a.ToResponse()).ToArray());

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            System.Globalization.CultureInfo.InvariantCulture);
}
