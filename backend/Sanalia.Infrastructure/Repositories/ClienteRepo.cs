using Microsoft.EntityFrameworkCore;
using Sanalia.Core.Entities;
using Sanalia.Core.Interfaces;
using Sanalia.Infrastructure.Data.Context;

namespace Sanalia.Infrastructure.Repositories;

public class ClienteRepository(SanaliaDbContext db) : Repository<Cliente>(db), IClienteRepository
{
    public Task<Cliente?> GetByCorreoAsync(string correo, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(c => c.Correo == correo, ct); // la columna es NOCASE

    public Task<Cliente?> GetByDocumentoAsync(string documentoIdentidad, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(c => c.DocumentoIdentidad == documentoIdentidad, ct);

    public async Task<IReadOnlyList<Cliente>> BuscarAsync(string texto, CancellationToken ct = default)
    {
        var patron = $"%{texto.Trim()}%";
        return await Set.AsNoTracking()
            .Where(c => EF.Functions.Like(c.Nombre, patron)
                     || EF.Functions.Like(c.DocumentoIdentidad, patron)
                     || EF.Functions.Like(c.Correo, patron)
                     || EF.Functions.Like(c.Telefono, patron))
            .OrderBy(c => c.Nombre)
            .ToListAsync(ct);
    }
}
