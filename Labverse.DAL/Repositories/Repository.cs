using Labverse.DAL.Data;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Labverse.DAL.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly LabverseDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(LabverseDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Remove(T entity)
    {
        entity.IsActive = false;
        _dbSet.Update(entity);
    }

    public IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }
}
