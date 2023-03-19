using Muuzika.Gateway.Entities;

namespace Muuzika.Gateway.Repositories.Interfaces;

public interface IServerRepository: IBaseEntityRepository<ServerEntity>
{
    Task<ServerEntity?> FindByNameAsync(string name);
}