using System.ComponentModel.DataAnnotations;
using Sanalia.Core.Enums;

namespace Sanalia.Api.Contracts;

/// <summary>Credenciales para iniciar una sesión.</summary>
public sealed record LoginRequest
{
    [Required, EmailAddress, MaxLength(254)]
    public required string Correo { get; init; }

    [Required, MinLength(1), MaxLength(256)]
    public required string Contraseña { get; init; }
}

/// <summary>Datos necesarios para registrar un cliente.</summary>
public sealed record RegisterClienteRequest
{
    [Required, MinLength(1), MaxLength(150)]
    public required string Nombre { get; init; }

    [Required, MinLength(1), MaxLength(50)]
    public required string DocumentoIdentidad { get; init; }

    [Required, MinLength(1), MaxLength(30)]
    public required string Telefono { get; init; }

    [Required, EmailAddress, MaxLength(254)]
    public required string Correo { get; init; }

    [Required, MinLength(8), MaxLength(128)]
    public required string Contraseña { get; init; }
}

/// <summary>Identidad autenticada y token JWT de acceso.</summary>
public sealed record AuthResponse(
    int Id,
    string Nombre,
    string Correo,
    bool EsAsesor,
    RolUsuario? Rol,
    string AccessToken,
    DateTimeOffset ExpiresAt);

/// <summary>Identidad asociada al token de acceso actual.</summary>
public sealed record CurrentUserResponse(
    int Id,
    string Nombre,
    string Correo,
    bool EsAsesor,
    RolUsuario? Rol);
