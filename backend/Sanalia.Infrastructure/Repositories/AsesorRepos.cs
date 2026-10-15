using Microsoft.EntityFrameworkCore;
using Sanalia.Core.Entities;
using Sanalia.Core.Interfaces;
using Sanalia.Infrastructure.Data.Context;

namespace Sanalia.Infrastructure.Repositories;

public class AsesorRepository(SanaliaDbContext db) : Repository<Asesor>(db), IAsesorRepository
{
    public Task<Asesor?> GetByCorreoAsync(string correo, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(a => a.Correo == correo, ct);
}
