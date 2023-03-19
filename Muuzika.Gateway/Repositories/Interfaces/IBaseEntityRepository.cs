using Muuzika.Gateway.Entities;

namespace Muuzika.Gateway.Repositories.Interfaces;

public interface IBaseEntityRepository<T> where T: BaseEntity
{
    void Add(T entity);
    void AddRange(IEnumerable<T> entities);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);
    Task SaveChangesAsync();
    Task<List<T>> GetAllAsync();
    ValueTask<T?> FindByIdAsync(int id);
    Task<List<T>> FindByIdsAsync(IEnumerable<int> ids);
}