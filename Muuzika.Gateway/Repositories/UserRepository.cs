using Microsoft.EntityFrameworkCore;
using Muuzika.Gateway.Database;
using Muuzika.Gateway.Entities;
using Muuzika.Gateway.Repositories.Interfaces;

namespace Muuzika.Gateway.Repositories;

public class UserRepository: BaseEntityRepository<UserEntity>, IUserRepository
{
    public UserRepository(MuuzikaDbContext dbContext) : base(dbContext)
    {
    }
    
    public Task<UserEntity?> FindByEmailAsync(string email)
    {
        return DbSet.FirstOrDefaultAsync(e => e.Email == email);
    }
}