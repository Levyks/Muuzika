using Microsoft.EntityFrameworkCore;
using Muuzika.Gateway.Database;
using Muuzika.Gateway.Entities;
using Muuzika.Gateway.Repositories.Interfaces;

namespace Muuzika.Gateway.Repositories;

public class BaseEntityRepository<T>: IBaseEntityRepository<T> where T: BaseEntity
{
    protected readonly MuuzikaDbContext DbContext;
    protected readonly DbSet<T> DbSet;

    protected BaseEntityRepository(MuuzikaDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<T>();
    }

    public void Add(T entity)
    {
        DbSet.Add(entity);
    }

    public void AddRange(IEnumerable<T> entities)
    {
        DbSet.AddRange(entities);
    }

    public void Update(T entity)
    {
        DbSet.Update(entity);
    }
    
    public void UpdateRange(IEnumerable<T> entities)
    {
        DbSet.UpdateRange(entities);
    }

    public void Delete(T entity)
    {
        DbSet.Remove(entity);
    }
    
    public void DeleteRange(IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);
    }

    public Task SaveChangesAsync()
    {
        return DbContext.SaveChangesAsync();
    }

    public Task<List<T>> GetAllAsync()
    {
        return DbSet.ToListAsync();
    }

    public ValueTask<T?> FindByIdAsync(int id)
    {
        return DbSet.FindAsync(id);
    }

    public Task<List<T>> FindByIdsAsync(IEnumerable<int> ids)
    {
        return DbSet.Where(e => ids.Contains(e.Id)).ToListAsync();
    }
}