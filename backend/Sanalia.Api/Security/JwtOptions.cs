namespace Sanalia.Api.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SigningKey { get; init; } = string.Empty;
    public int LifetimeMinutes { get; init; } = 60;
}

internal static class JwtRoles
{
    public const string Cliente = "Cliente";
    public const string Asesor = "Asesor";
    public const string Administrador = "Administrador";
    public const string Personal = Asesor + "," + Administrador;
}
