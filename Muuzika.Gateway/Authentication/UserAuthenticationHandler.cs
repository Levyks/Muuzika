using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Muuzika.Gateway.Enums;
using Muuzika.Gateway.Repositories.Interfaces;
using Muuzika.Gateway.Services.Interfaces;

namespace Muuzika.Gateway.Authentication;

public class UserAuthenticationHandler: AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userRepository;
    
    public UserAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder, 
        ISystemClock clock,
        IJwtService jwtService,
        IUserRepository userRepository) : base(options, logger, encoder, clock)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorizationHeader = Request.Headers["Authorization"].ToString();

        var prefix = "Bearer ";
        
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith(prefix))
            return AuthenticateResult.Fail("Authorization header is invalid");
        
        var token = authorizationHeader.Substring(prefix.Length);
        
        var jwtPrincipal = _jwtService.GetPrincipalFromToken(token, out var validatedToken);
        
        if (jwtPrincipal is null || validatedToken is null)
            return AuthenticateResult.Fail("Invalid token");
        
        var nameIdentifier = jwtPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (nameIdentifier is null || !int.TryParse(nameIdentifier, out var userId))
            return AuthenticateResult.Fail("Invalid token");
        
        var user = await _userRepository.FindByIdAsync(userId);
        
        if (user is null || user.LastTokenInvalidation > validatedToken.ValidFrom)
            return AuthenticateResult.Fail("Invalid token");
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, AuthenticatableTypeEnum.User.ToString()),
            new(ClaimTypes.UserData, JsonSerializer.Serialize(user))
        };
        
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        
        return AuthenticateResult.Success(ticket);
    }
}