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
    IAsesoriaService asesorias) : ControllerBase
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
        var asesoria = await asesorias.CrearAsync(
            request.SolicitudId,
            GetUserId(),
            request.FechaAsesoria.UtcDateTime,
            request.NecesidadesIdentificadas,
            request.Notas,
            cancellationToken);
        var response = asesoria.ToResponse();
        return Created("/api/asesorias/mias", response);
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
