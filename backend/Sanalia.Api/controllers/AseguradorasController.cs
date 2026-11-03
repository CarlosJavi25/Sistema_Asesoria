using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sanalia.Api.Contracts;
using Sanalia.Api.Mappings;
using Sanalia.Api.Security;
using Sanalia.Core.Interfaces.IServices;

namespace Sanalia.Api.Controllers;

[ApiController]
[Route("api/aseguradoras")]
[Authorize(Roles = JwtRoles.Administrador)]
[Produces("application/json")]
public sealed class AseguradorasController(IAseguradoraService aseguradoras) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AseguradoraResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AseguradoraResponse>>> GetAll(
        CancellationToken cancellationToken) =>
        Ok((await aseguradoras.GetAllAsync(cancellationToken))
            .Select(item => item.ToResponse()).ToArray());

    [HttpGet("{id:int}")]
    [ProducesResponseType<AseguradoraResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AseguradoraResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var aseguradora = await aseguradoras.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe la aseguradora {id}.");
        return Ok(aseguradora.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType<AseguradoraResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AseguradoraResponse>> Create(
        SaveAseguradoraRequest request,
        CancellationToken cancellationToken)
    {
        var created = await aseguradoras.CreateAsync(
            request.Nombre, request.Telefono, request.CorreoContacto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToResponse());
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<AseguradoraResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AseguradoraResponse>> Update(
        int id,
        SaveAseguradoraRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await aseguradoras.UpdateAsync(
            id, request.Nombre, request.Telefono, request.CorreoContacto, cancellationToken);
        return Ok(updated.ToResponse());
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await aseguradoras.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
