using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sanalia.Api.Contracts;
using Sanalia.Api.Mappings;
using Sanalia.Api.Security;
using Sanalia.Core.Enums;
using Sanalia.Core.Interfaces.IServices;

namespace Sanalia.Api.Controllers;

[ApiController]
[Route("api/solicitudes-asistencia")]
[Produces("application/json")]
public sealed class SolicitudesAsistenciaController(
    ISolicitudAsistenciaService solicitudes) : ControllerBase
{
    [Authorize(Roles = JwtRoles.Cliente)]
    [HttpPost]
    [ProducesResponseType<SolicitudAsistenciaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudAsistenciaResponse>> Create(
        CreateSolicitudAsistenciaRequest request,
        CancellationToken cancellationToken)
    {
        var solicitud = await solicitudes.CrearAsync(GetUserId(), request.Motivo, cancellationToken);
        return Created("/api/solicitudes-asistencia/mias", solicitud.ToResponse());
    }

    [Authorize(Roles = JwtRoles.Cliente)]
    [HttpGet("mias")]
    [ProducesResponseType<IReadOnlyList<SolicitudAsistenciaResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SolicitudAsistenciaResponse>>> GetMine(
        CancellationToken cancellationToken) =>
        Ok((await solicitudes.GetByClienteAsync(GetUserId(), cancellationToken))
            .Select(s => s.ToResponse()).ToArray());

    [Authorize(Roles = JwtRoles.Personal)]
    [HttpGet("abiertas")]
    [ProducesResponseType<IReadOnlyList<SolicitudAsistenciaResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SolicitudAsistenciaResponse>>> GetOpen(
        CancellationToken cancellationToken)
    {
        var abiertas = await solicitudes.GetAbiertasAsync(cancellationToken);
        if (User.IsInRole(JwtRoles.Asesor) && !User.IsInRole(JwtRoles.Administrador))
            abiertas = abiertas.Where(s => s.AsesorId is null || s.AsesorId == GetUserId()).ToArray();
        return Ok(abiertas.Select(s => s.ToResponse()).ToArray());
    }

    [Authorize(Roles = JwtRoles.Personal)]
    [HttpGet("personal")]
    [ProducesResponseType<IReadOnlyList<SolicitudAsistenciaResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SolicitudAsistenciaResponse>>> GetAllForStaff(
        CancellationToken cancellationToken)
    {
        var todas = await solicitudes.GetTodasAsync(cancellationToken);
        if (User.IsInRole(JwtRoles.Asesor) && !User.IsInRole(JwtRoles.Administrador))
            todas = todas.Where(s => s.AsesorId is null || s.AsesorId == GetUserId()).ToArray();
        return Ok(todas.Select(s => s.ToResponse()).ToArray());
    }

    [Authorize(Roles = JwtRoles.Personal)]
    [HttpPost("{id:int}/asignacion")]
    [ProducesResponseType<SolicitudAsistenciaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SolicitudAsistenciaResponse>> Assign(
        int id,
        CancellationToken cancellationToken)
    {
        var solicitud = await solicitudes.AsignarAsync(id, GetUserId(), cancellationToken);
        return Ok(solicitud.ToResponse());
    }

    [Authorize(Roles = JwtRoles.Personal)]
    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType<SolicitudAsistenciaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SolicitudAsistenciaResponse>> ChangeState(
        int id,
        ChangeAsistenciaStateRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Estado != EstadoAsistencia.Resuelta)
            throw new ArgumentException("El único cambio de estado permitido es marcarla como resuelta.");

        var solicitud = await solicitudes.CambiarEstadoAsync(
            id, GetUserId(), request.Estado, cancellationToken);
        return Ok(solicitud.ToResponse());
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            System.Globalization.CultureInfo.InvariantCulture);
}
