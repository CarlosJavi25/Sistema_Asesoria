using Sanalia.Core.Entities;
using Sanalia.Core.Interfaces;
using Sanalia.Core.Interfaces.IServices;

namespace Sanalia.Infrastructure.Services;

public sealed class ClienteService(IClienteRepository clientes) : IClienteService
{
    public async Task<IReadOnlyList<Cliente>> GetAllAsync(
        string? search = null,
        CancellationToken ct = default)
    {
        var results = string.IsNullOrWhiteSpace(search)
            ? await clientes.GetAllAsync(ct)
            : await clientes.BuscarAsync(search, ct);

        return results.OrderBy(cliente => cliente.Nombre).ToArray();
    }
}
