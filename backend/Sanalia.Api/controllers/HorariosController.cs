using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sanalia.Api.Contracts;
using Sanalia.Api.Mappings;
using Sanalia.Api.Security;
using Sanalia.Core.Interfaces.IServices;

namespace Sanalia.Api.Controllers;

[ApiController]
[Route("api/horarios")]
[Produces("application/json")]
public sealed class HorariosController(IHorarioDisponibleService horarios) : ControllerBase
{
    [HttpGet("disponibles")]
    [ProducesResponseType<IReadOnlyList<HorarioDisponibleResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HorarioDisponibleResponse>>> GetDisponibles(
        [FromQuery] DateOnly? desde,
        [FromQuery] int? asesorId,
        CancellationToken cancellationToken)
    {
        var fechaDesde = desde ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var disponibles = await horarios.GetDisponiblesAsync(fechaDesde, asesorId, cancellationToken);
        return Ok(disponibles.Select(h => h.ToResponse()).ToArray());
    }

    [Authorize(Roles = JwtRoles.Personal)]
    [HttpPost]
    [ProducesResponseType<HorarioDisponibleResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HorarioDisponibleResponse>> Create(
        CreateHorarioRequest request,
        CancellationToken cancellationToken)
    {
        var idAsesor = GetUserId();
        var horario = await horarios.CrearAsync(
            idAsesor,
            request.Fecha,
            request.HoraInicio,
            request.HoraFin,
            cancellationToken);
        var response = new HorarioDisponibleResponse(
            horario.Id,
            horario.AsesorId,
            User.FindFirstValue(ClaimTypes.Name)!,
            horario.Fecha,
            horario.HoraInicio,
            horario.HoraFin);
        return CreatedAtAction(nameof(GetDisponibles), new { desde = horario.Fecha }, response);
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            System.Globalization.CultureInfo.InvariantCulture);
}
