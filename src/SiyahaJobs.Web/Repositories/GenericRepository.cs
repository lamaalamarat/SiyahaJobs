using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SiyahaJobs.Web.Data;
using SiyahaJobs.Web.Repositories.Interfaces;

namespace SiyahaJobs.Web.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _db;
    protected readonly DbSet<T> _set;

    public GenericRepository(ApplicationDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public IQueryable<T> Query() => _set.AsQueryable();

    public Task<T?> GetByIdAsync(object id) => _set.FindAsync(id).AsTask();

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
        _set.FirstOrDefaultAsync(predicate);

    public async Task<IReadOnlyList<T>> ListAllAsync() =>
        await _set.AsNoTracking().ToListAsync();

    public async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> predicate) =>
        await _set.AsNoTracking().Where(predicate).ToListAsync();

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) =>
        _set.AnyAsync(predicate);

    public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) =>
        predicate is null ? _set.CountAsync() : _set.CountAsync(predicate);

    public async Task AddAsync(T entity) => await _set.AddAsync(entity);

    public async Task AddRangeAsync(IEnumerable<T> entities) =>
        await _set.AddRangeAsync(entities);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);

    public void RemoveRange(IEnumerable<T> entities) => _set.RemoveRange(entities);
}
