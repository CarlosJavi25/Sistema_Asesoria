using Sanalia.Core.Entities;

namespace Sanalia.Core.Interfaces.IServices;

public interface IAseguradoraService
{
    Task<IReadOnlyList<Aseguradora>> GetAllAsync(CancellationToken ct = default);
    Task<Aseguradora?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Aseguradora> CreateAsync(
        string nombre,
        string? telefono,
        string? correoContacto,
        CancellationToken ct = default);
    Task<Aseguradora> UpdateAsync(
        int id,
        string nombre,
        string? telefono,
        string? correoContacto,
        CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
