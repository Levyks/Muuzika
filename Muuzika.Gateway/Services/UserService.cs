using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Muuzika.Gateway.Database;
using Muuzika.Gateway.Entities;
using Muuzika.Gateway.Repositories.Interfaces;
using Muuzika.Gateway.Services.Interfaces;

namespace Muuzika.Gateway.Services;

public class UserService: IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IHashService _hashService;

    public UserService(IUserRepository userRepository, IHashService hashService)
    {
        _userRepository = userRepository;
        _hashService = hashService;
    }

    public async Task<UserEntity?> AuthenticateAsync(string email, string password)
    {
        var user = await _userRepository.FindByEmailAsync(email);
        
        if (user == null || !_hashService.Verify(password, user.Password))
            return null;

        return user;
    }

    public ClaimsIdentity GenerateClaimsIdentity(UserEntity user)
    {
        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, user.Id.ToString()),
            new (ClaimTypes.Name, user.Name),
            new (ClaimTypes.Role, user.Type.ToString()),
            new (ClaimTypes.UserData, JsonSerializer.Serialize(this))
        };
        
        return new ClaimsIdentity(claims);
    }
}