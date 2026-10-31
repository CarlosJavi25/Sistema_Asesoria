using Sanalia.Core.Entities;

namespace Sanalia.Core.Interfaces.IServices;

public interface IClienteService
{
    Task<IReadOnlyList<Cliente>> GetAllAsync(string? search = null, CancellationToken ct = default);
}
