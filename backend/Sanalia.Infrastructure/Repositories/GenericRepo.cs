using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Sanalia.Core.Interfaces;
using Sanalia.Infrastructure.Data.Context;

namespace Sanalia.Infrastructure.Repositories;

public class Repository<T>(SanaliaDbContext db) : IRepository<T> where T : class
{
    protected readonly SanaliaDbContext Db = db;

    protected DbSet<T> Set => Db.Set<T>();

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await Set.FindAsync(new object[] { id }, ct);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await Set.AsNoTracking().ToListAsync(ct);

    public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await Set.AsNoTracking().Where(predicate).ToListAsync(ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default) =>
        await Set.AddAsync(entity, ct);

    public virtual void Update(T entity) => Set.Update(entity);

    public virtual void Remove(T entity) => Set.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Db.SaveChangesAsync(ct);
}