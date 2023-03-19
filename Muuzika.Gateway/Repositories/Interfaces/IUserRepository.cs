using Muuzika.Gateway.Entities;

namespace Muuzika.Gateway.Repositories.Interfaces;

public interface IUserRepository: IBaseEntityRepository<UserEntity>
{
    Task<UserEntity?> FindByEmailAsync(string email);
}