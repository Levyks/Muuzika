using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Muuzika.Server.Exceptions;
using Muuzika.Server.Extensions.Room;
using Muuzika.Server.Models;

namespace Muuzika.Server.Hubs.Extensions;

public static class RoomHubAuthExtensions
{
    private static ClaimsPrincipal ParseTokenFromQuery(this RoomHub hub)
    {
        const string headerPrefix = "Bearer ";
        var httpContext = hub.Context.GetHttpContext();
        
        if (
            httpContext == null ||
            !httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader) ||
            !authHeader.ToString().StartsWith(headerPrefix))
        {
            throw new NoTokenProvidedException();
        }
        
        var token = authHeader.ToString()[headerPrefix.Length..].Trim();

        var principal = hub.JwtService.GetPrincipalFromToken(token);
        
        if (principal == null)
        {
            throw new InvalidTokenException();
        }

        return principal;
    }
    
    public static Player ParseTokenAndGetPlayer(this RoomHub hub)
    {
        var principal = hub.ParseTokenFromQuery();
        
        var roomCode = principal.Claims.FirstOrDefault(x => x.Type == "roomCode")?.Value;
        var username = principal.Claims.FirstOrDefault(x => x.Type == "username")?.Value;
        
        if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(username))
            throw new InvalidTokenException();

        var room = hub.RoomRepository.GetRoomByCode(roomCode);
        return room.GetPlayer(username);
    }
}