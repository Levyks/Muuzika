using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Muuzika.Gateway.Enums;
using Muuzika.Gateway.Repositories.Interfaces;
using Muuzika.Gateway.Services.Interfaces;

namespace Muuzika.Gateway.Authentication;

public class ServerAuthenticationHandler: AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IServerRepository _serverRepository;
    
    public ServerAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder, 
        ISystemClock clock,
        IServerRepository serverRepository) : base(options, logger, encoder, clock)
    {
        _serverRepository = serverRepository;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorizationHeader = Request.Headers["Authorization"].ToString();

        var prefix = "Basic ";
        
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith(prefix))
            return AuthenticateResult.Fail("Authorization header is invalid");
        
        var parameter = authorizationHeader.Substring(prefix.Length);
        var parameterParts = Encoding.UTF8.GetString(Convert.FromBase64String(parameter)).Split(':');
        
        if (parameterParts.Length != 2)
            return AuthenticateResult.Fail("Authorization header is invalid");
        
        var name = parameterParts[0];
        var token = parameterParts[1];

        var server = await _serverRepository.FindByNameAsync(name);
        
        if (server is null || server.Token != token)
            return AuthenticateResult.Fail("Invalid server credentials");
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, server.Id.ToString()),
            new(ClaimTypes.Name, server.Name),
            new(ClaimTypes.Role, AuthenticatableTypeEnum.Server.ToString()),
            new(ClaimTypes.UserData, JsonSerializer.Serialize(server))
        };
        
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        
        return AuthenticateResult.Success(ticket);
    }
}