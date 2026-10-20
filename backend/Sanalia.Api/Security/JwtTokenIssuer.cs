using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sanalia.Core.Models;

namespace Sanalia.Api.Security;

internal sealed class JwtTokenIssuer(IOptions<JwtOptions> options) : ITokenIssuer
{
    public (string Token, DateTimeOffset ExpiresAt) Issue(UserIdentity identity)
    {
        var config = options.Value;
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(config.LifetimeMinutes);
        var role = identity.EsAsesor
            ? identity.Rol?.ToString() ?? throw new InvalidOperationException("El asesor no tiene un rol válido.")
            : JwtRoles.Cliente;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, identity.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.Name, identity.Nombre),
            new Claim(ClaimTypes.Email, identity.Correo),
            new Claim(ClaimTypes.Role, role),
            new Claim("user_type", identity.EsAsesor ? "asesor" : "cliente")
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: config.Issuer,
            audience: config.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
