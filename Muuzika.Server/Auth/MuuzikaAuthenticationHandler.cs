using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Muuzika.Server.Exceptions;
using Muuzika.Server.Repositories.Interfaces;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Auth;

public class MuuzikaAuthenticationHandler: AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IJwtService _jwtService;
    private readonly IRoomRepository _roomRepository;
    
    public MuuzikaAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder, 
        ISystemClock clock,
        IJwtService jwtService,
        IRoomRepository roomRepository) : base(options, logger, encoder, clock)
    {
        _jwtService = jwtService;
        _roomRepository = roomRepository;
    }
    
    private AuthenticateResult HandleAuthenticate()
    {
        var authorizationHeader = Request.Headers["Authorization"].ToString();

        const string prefix = "Bearer ";

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith(prefix))
            return AuthenticateResult.NoResult();
        
        var token = authorizationHeader.Substring(prefix.Length);
        
        var principal = _jwtService.GetPrincipalFromToken(token);
        
        if (principal is null)
            return AuthenticateResult.Fail(new InvalidTokenException());
        
        var roomCode = principal.Claims.FirstOrDefault(x => x.Type == "roomCode")?.Value;
        var username = principal.Claims.FirstOrDefault(x => x.Type == "username")?.Value;
        
        if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(username))
            return AuthenticateResult.Fail(new InvalidTokenException());

        var room = _roomRepository.FindRoomByCode(roomCode);
        
        if (room is null)
            return AuthenticateResult.Fail(new RoomNotFoundException(roomCode));
        
        var roomPlayerService = room.ServiceProvider.GetRequiredService<IRoomPlayerService>();
        
        var player = roomPlayerService.FindPlayer(username);

        if (player is null)
            return AuthenticateResult.Fail(new PlayerNotFoundException(roomCode, username));
        
        Request.HttpContext.Items["player"] = player;
        
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        
        return AuthenticateResult.Success(ticket);
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return Task.FromResult(HandleAuthenticate());
    }
}