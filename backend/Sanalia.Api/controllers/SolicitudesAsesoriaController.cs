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
[Route("api/solicitudes-asesoria")]
[Produces("application/json")]
public sealed class SolicitudesAsesoriaController(
    ISolicitudAsesoriaService solicitudes,
    IProductoSeguroService productos) : ControllerBase
{
    [Authorize(Roles = JwtRoles.Cliente)]
    [HttpPost]
    [ProducesResponseType<SolicitudAsesoriaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SolicitudAsesoriaResponse>> Create(
        CreateSolicitudAsesoriaRequest request,
        CancellationToken cancellationToken)
    {
        var clienteId = GetUserId();
        var solicitud = await solicitudes.CrearAsync(
            clienteId,
            request.TipoId,
            request.HorarioId,
            request.Presupuesto,
            request.DescripcionNecesidad,
            cancellationToken);
        var response = await solicitudes.GetByIdAsync(solicitud.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe la solicitud de asesoría {solicitud.Id}.");
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response.ToResponse());
    }

    [Authorize(Roles = JwtRoles.Cliente)]
    [HttpGet("mias")]
    [ProducesResponseType<IReadOnlyList<SolicitudAsesoriaResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SolicitudAsesoriaResponse>>> GetMine(
        CancellationToken cancellationToken) =>
        Ok((await solicitudes.GetByClienteAsync(GetUserId(), cancellationToken))
            .Select(s => s.ToResponse()).ToArray());

    [Authorize]
    [HttpGet("{id:int}")]
    [ProducesResponseType<SolicitudAsesoriaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudAsesoriaResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var solicitud = await GetAuthorizedSolicitud(id, cancellationToken);
        return Ok(solicitud.ToResponse());
    }

    [Authorize(Roles = JwtRoles.Personal)]
    [HttpGet("pendientes")]
    [ProducesResponseType<IReadOnlyList<SolicitudAsesoriaResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SolicitudAsesoriaResponse>>> GetPending(
        CancellationToken cancellationToken)
    {
        var pendientes = await solicitudes.GetPendientesAsync(cancellationToken);
        if (User.IsInRole(JwtRoles.Asesor) && !User.IsInRole(JwtRoles.Administrador))
            pendientes = pendientes.Where(s => s.Horario?.AsesorId == GetUserId()).ToArray();
        return Ok(pendientes.Select(s => s.ToResponse()).ToArray());
    }

    [Authorize(Roles = JwtRoles.Personal)]
    [HttpGet("operativas")]
    [ProducesResponseType<IReadOnlyList<SolicitudAsesoriaResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SolicitudAsesoriaResponse>>> GetOperational(
        CancellationToken cancellationToken)
    {
        var operational = await solicitudes.GetOperativasAsync(cancellationToken);
        if (User.IsInRole(JwtRoles.Asesor) && !User.IsInRole(JwtRoles.Administrador))
            operational = operational.Where(s => s.Horario?.AsesorId == GetUserId()).ToArray();
        return Ok(operational.Select(s => s.ToResponse()).ToArray());
    }

    [Authorize(Roles = JwtRoles.Cliente)]
    [HttpGet("{id:int}/alternativas")]
    [ProducesResponseType<IReadOnlyList<AlternativaSeguroResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AlternativaSeguroResponse>>> GetAlternatives(
        int id,
        CancellationToken cancellationToken)
    {
        var solicitud = (await solicitudes.GetByClienteAsync(GetUserId(), cancellationToken))
            .FirstOrDefault(s => s.Id == id)
            ?? throw new KeyNotFoundException($"No existe la solicitud de asesoría {id}.");
        var alternatives = await productos.CompararAsync(solicitud.Id, cancellationToken);
        return Ok(alternatives.Select(a => a.ToResponse()).ToArray());
    }

    [Authorize(Roles = JwtRoles.Personal)]
    [HttpGet("{id:int}/comparador")]
    [ProducesResponseType<IReadOnlyList<AlternativaSeguroResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AlternativaSeguroResponse>>> GetStaffAlternatives(
        int id,
        CancellationToken cancellationToken)
    {
        _ = await GetAuthorizedSolicitud(id, cancellationToken);
        var alternatives = await productos.CompararAsync(id, cancellationToken);
        return Ok(alternatives.Select(a => a.ToResponse()).ToArray());
    }

    [Authorize]
    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType<SolicitudAsesoriaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SolicitudAsesoriaResponse>> ChangeState(
        int id,
        ChangeSolicitudAsesoriaStateRequest request,
        CancellationToken cancellationToken)
    {
        var solicitud = await GetAuthorizedSolicitud(id, cancellationToken);
        if (User.IsInRole(JwtRoles.Cliente))
        {
            if (request.Estado != EstadoSolicitudAsesoria.Cancelada)
                throw new UnauthorizedAccessException("Los clientes solo pueden cancelar sus propias solicitudes.");
        }

        _ = await solicitudes.CambiarEstadoAsync(id, request.Estado, cancellationToken);
        var updated = await solicitudes.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe la solicitud de asesoría {id}.");
        return Ok(updated.ToResponse());
    }

    private async Task<Sanalia.Core.Entities.SolicitudAsesoria> GetAuthorizedSolicitud(
        int id,
        CancellationToken cancellationToken)
    {
        var solicitud = await solicitudes.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe la solicitud de asesoría {id}.");

        if (User.IsInRole(JwtRoles.Cliente))
        {
            if (solicitud.ClienteId == GetUserId())
                return solicitud;
            throw new KeyNotFoundException($"No existe la solicitud de asesoría {id}.");
        }
        if (User.IsInRole(JwtRoles.Asesor)
            && solicitud.Horario?.AsesorId == GetUserId())
            return solicitud;
        if (User.IsInRole(JwtRoles.Administrador))
            return solicitud;

        throw new UnauthorizedAccessException();
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            System.Globalization.CultureInfo.InvariantCulture);
}
