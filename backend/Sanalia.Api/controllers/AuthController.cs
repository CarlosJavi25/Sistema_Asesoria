using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sanalia.Api.Contracts;
using Sanalia.Api.Mappings;
using Sanalia.Api.Security;
using Sanalia.Core.Interfaces.IServices;

namespace Sanalia.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController(
    IAuthService authService,
    ITokenIssuer tokenIssuer) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterClienteRequest request,
        CancellationToken cancellationToken)
    {
        var identity = await authService.RegisterClienteAsync(
            request.Nombre,
            request.DocumentoIdentidad,
            request.Telefono,
            request.Correo,
            request.Contraseña,
            cancellationToken);
        var (token, expiresAt) = tokenIssuer.Issue(identity);
        return Created("/api/auth/me", new AuthResponse(
            identity.Id,
            identity.Nombre,
            identity.Correo,
            identity.EsAsesor,
            identity.Rol,
            token,
            expiresAt));
    }

    [HttpPost("login/cliente")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> LoginCliente(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var identity = await authService.AuthenticateClienteAsync(
            request.Correo, request.Contraseña, cancellationToken);
        return identity is null
            ? Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid credentials")
            : Ok(CreateAuthResponse(identity));
    }

    [HttpPost("login/asesor")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> LoginAsesor(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var identity = await authService.AuthenticateAsesorAsync(
            request.Correo, request.Contraseña, cancellationToken);
        return identity is null
            ? Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid credentials")
            : Ok(CreateAuthResponse(identity));
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public ActionResult<CurrentUserResponse> GetCurrent()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            System.Globalization.CultureInfo.InvariantCulture);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var isAdvisor = role is JwtRoles.Asesor or JwtRoles.Administrador;
        var response = new CurrentUserResponse(
            id,
            User.FindFirstValue(ClaimTypes.Name)!,
            User.FindFirstValue(ClaimTypes.Email)!,
            isAdvisor,
            isAdvisor && Enum.TryParse<Sanalia.Core.Enums.RolUsuario>(role, out var parsedRole)
                ? parsedRole
                : null);
        return Ok(response);
    }

    private AuthResponse CreateAuthResponse(Sanalia.Core.Models.UserIdentity identity)
    {
        var (token, expiresAt) = tokenIssuer.Issue(identity);
        return new AuthResponse(
            identity.Id,
            identity.Nombre,
            identity.Correo,
            identity.EsAsesor,
            identity.Rol,
            token,
            expiresAt);
    }
}
