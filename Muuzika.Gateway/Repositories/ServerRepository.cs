using Microsoft.EntityFrameworkCore;
using Muuzika.Gateway.Database;
using Muuzika.Gateway.Entities;
using Muuzika.Gateway.Repositories.Interfaces;

namespace Muuzika.Gateway.Repositories;

public class ServerRepository: BaseEntityRepository<ServerEntity>, IServerRepository
{
    public ServerRepository(MuuzikaDbContext dbContext) : base(dbContext)
    {
    }
    
    public Task<ServerEntity?> FindByNameAsync(string name)
    {
        return DbSet.FirstOrDefaultAsync(e => e.Name == name);
    }
}