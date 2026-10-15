using Microsoft.EntityFrameworkCore;
using Sanalia.Core.Entities;
using Sanalia.Core.Interfaces;
using Sanalia.Infrastructure.Data.Context;

namespace Sanalia.Infrastructure.Repositories;

public class ProductoSeguroRepository(SanaliaDbContext db) : Repository<ProductoSeguro>(db), IProductoSeguroRepository
{

    // Sin OrderBy por Prima a propósito: el comparador ordena por puntaje en memoria.
    public async Task<IReadOnlyList<ProductoSeguro>> GetVigentesPorTipoAsync(int idTipo, CancellationToken ct = default) =>
        await Set.AsNoTracking()
            .Include(p => p.Aseguradora)
            .Include(p => p.Coberturas)
            .Where(p => p.Vigente && p.TipoId == idTipo)
            .AsSplitQuery()
            .ToListAsync(ct);

    public Task<ProductoSeguro?> GetConDetalleAsync(int id, CancellationToken ct = default) =>
        Set.Include(p => p.Aseguradora)
            .Include(p => p.TipoSeguro)
            .Include(p => p.Coberturas)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken: ct);
}
