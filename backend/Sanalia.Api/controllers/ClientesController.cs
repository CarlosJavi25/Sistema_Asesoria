using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sanalia.Api.Contracts;
using Sanalia.Api.Mappings;
using Sanalia.Api.Security;
using Sanalia.Core.Interfaces.IServices;

namespace Sanalia.Api.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize(Roles = JwtRoles.Personal)]
[Produces("application/json")]
public sealed class ClientesController(IClienteService clientes) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ClienteResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<ClienteResponse>>> GetAll(
        [FromQuery(Name = "busqueda"), MaxLength(100)] string? search,
        CancellationToken cancellationToken) =>
        Ok((await clientes.GetAllAsync(search, cancellationToken))
            .Select(cliente => cliente.ToResponse()).ToArray());
}
