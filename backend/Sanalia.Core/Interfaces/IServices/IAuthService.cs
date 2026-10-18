using Sanalia.Core.Models;

namespace Sanalia.Core.Interfaces.IServices;

public interface IAuthService
{
    Task<UserIdentity> RegisterClienteAsync(
        string nombre,
        string documentoIdentidad,
        string telefono,
        string correo,
        string contraseña,
        CancellationToken ct = default);

    Task<UserIdentity?> AuthenticateClienteAsync(string correo, string contraseña, CancellationToken ct = default);
    Task<UserIdentity?> AuthenticateAsesorAsync(string correo, string contraseña, CancellationToken ct = default);
}
