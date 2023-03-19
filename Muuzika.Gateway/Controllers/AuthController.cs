using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Muuzika.Gateway.Dtos.Auth;
using Muuzika.Gateway.Entities;
using Muuzika.Gateway.Enums;
using Muuzika.Gateway.Services.Interfaces;

namespace Muuzika.Gateway.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthController: ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    
    public AuthController(IUserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    [HttpPost]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var user = await _userService.AuthenticateAsync(request.Email, request.Password);
        if (user == null)
            return Unauthorized("Invalid credentials");

        var identity = _userService.GenerateClaimsIdentity(user);
        var token = _jwtService.GenerateToken(identity);
        return new LoginResponseDto(token, user);
    }
    
    [HttpGet]
    [Authorize]
    public IActionResult WhoAmI()
    {
        var userJson = HttpContext.User.FindFirst(ClaimTypes.UserData)?.Value;

        if (userJson == null || !Enum.TryParse(HttpContext.User.FindFirst(ClaimTypes.Role)?.Value, out AuthenticatableTypeEnum authenticatableType))
            return Unauthorized();
        
        AuthenticatableEntity? entity = authenticatableType switch
        {
            AuthenticatableTypeEnum.User => JsonSerializer.Deserialize<UserEntity>(userJson),
            AuthenticatableTypeEnum.Server => JsonSerializer.Deserialize<ServerEntity>(userJson),
            _ => null
        };
        
        return Ok(entity);
    }
}