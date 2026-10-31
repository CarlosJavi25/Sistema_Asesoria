using Sanalia.Core.Enums;

namespace Sanalia.Core.Models;

public sealed record UserIdentity(
    int Id,
    string Nombre,
    string Correo,
    bool EsAsesor,
    RolUsuario? Rol);
