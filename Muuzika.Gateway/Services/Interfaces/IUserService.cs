using System.Security.Claims;
using Muuzika.Gateway.Entities;

namespace Muuzika.Gateway.Services.Interfaces;

public interface IUserService
{
    Task<UserEntity?> AuthenticateAsync(string email, string password);
    ClaimsIdentity GenerateClaimsIdentity(UserEntity user);
}