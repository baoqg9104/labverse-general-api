using Labverse.DAL.EntitiesModels;

namespace Labverse.DAL.Repositories.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    IQueryable<T> Query();
}