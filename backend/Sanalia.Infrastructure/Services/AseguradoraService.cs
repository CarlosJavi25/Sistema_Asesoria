using Sanalia.Core.Entities;
using Sanalia.Core.Interfaces;
using Sanalia.Core.Interfaces.IServices;

namespace Sanalia.Infrastructure.Services;

public sealed class AseguradoraService(
    IRepository<Aseguradora> aseguradoras,
    IRepository<ProductoSeguro> productos) : IAseguradoraService
{
    public Task<IReadOnlyList<Aseguradora>> GetAllAsync(CancellationToken ct = default) =>
        aseguradoras.GetAllAsync(ct);

    public Task<Aseguradora?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));

        return aseguradoras.GetByIdAsync(id, ct);
    }

    public async Task<Aseguradora> CreateAsync(
        string nombre,
        string? telefono,
        string? correoContacto,
        CancellationToken ct = default)
    {
        var normalizedName = NormalizeName(nombre);
        await EnsureNameIsAvailableAsync(normalizedName, null, ct);

        var aseguradora = new Aseguradora
        {
            Nombre = normalizedName,
            Telefono = NormalizeOptional(telefono),
            CorreoContacto = NormalizeOptional(correoContacto)
        };
        await aseguradoras.AddAsync(aseguradora, ct);
        await aseguradoras.SaveChangesAsync(ct);
        return aseguradora;
    }

    public async Task<Aseguradora> UpdateAsync(
        int id,
        string nombre,
        string? telefono,
        string? correoContacto,
        CancellationToken ct = default)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));

        var aseguradora = await aseguradoras.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No existe la aseguradora {id}.");
        var normalizedName = NormalizeName(nombre);
        await EnsureNameIsAvailableAsync(normalizedName, id, ct);

        aseguradora.Nombre = normalizedName;
        aseguradora.Telefono = NormalizeOptional(telefono);
        aseguradora.CorreoContacto = NormalizeOptional(correoContacto);
        await aseguradoras.SaveChangesAsync(ct);
        return aseguradora;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));

        var aseguradora = await aseguradoras.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No existe la aseguradora {id}.");
        if ((await productos.FindAsync(producto => producto.AseguradoraId == id, ct)).Count > 0)
            throw new InvalidOperationException(
                "No se puede eliminar una aseguradora que tiene productos de seguro asociados.");

        aseguradoras.Remove(aseguradora);
        await aseguradoras.SaveChangesAsync(ct);
    }

    private async Task EnsureNameIsAvailableAsync(
        string nombre,
        int? currentId,
        CancellationToken ct)
    {
        var duplicate = await aseguradoras.FindAsync(
            item => item.Nombre == nombre && (!currentId.HasValue || item.Id != currentId.Value),
            ct);
        if (duplicate.Count > 0)
            throw new InvalidOperationException("Ya existe una aseguradora con ese nombre.");
    }

    private static string NormalizeName(string nombre)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        return nombre.Trim();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
