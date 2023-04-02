using System.Collections.Immutable;
using System.Security.Claims;
using Muuzika.Server.Exceptions;
using Muuzika.Server.Models;

namespace Muuzika.Server.Extensions.Room;

public static class RoomPlayerRegistrationExtensions
{
    public static ImmutableArray<Player> GetPlayers(this Models.Room room)
    {
        return room.PlayersDictionary.Values.ToImmutableArray();
    }
    
    public static ImmutableArray<Player> GetConnectedPlayers(this Models.Room room)
    {
        return room.PlayersDictionary.Values.Where(p => p.IsConnected).ToImmutableArray();
    }
    
    public static bool HasConnectedPlayers(this Models.Room room)
    {
        return room.PlayersDictionary.Values.Any(p => p.IsConnected);
    }
    
    public static Player? FindPlayer(this Models.Room room, string username)
    {
        return room.PlayersDictionary.TryGetValue(username, out var player) ? player : null;
    }
    
    public static Player GetPlayer(this Models.Room room, string username)
    {
        return room.FindPlayer(username) ?? throw new PlayerNotFoundException(room.Code, username);
    }
    
    public static Player AddPlayer(this Models.Room room, Models.Player player)
    {
        room.PlayersDictionary.Add(player.Username, player);
        room.Logger.Information("Player {Username} joined the room", player.Username);
        return player;
    }
    
    public static Models.Player AddPlayer(this Models.Room room, string username)
    {
        var player = new Models.Player(room, username);
        return room.AddPlayer(player);
    }

    public static bool RemovePlayer(this Models.Room room, string username)
    {
        room.Logger.Information("Player {Username} left the room", username);
        return room.PlayersDictionary.Remove(username);
    }
    
    public static string GetTokenForPlayer(this Models.Room room, Models.Player player)
    {
        if (!room.PlayersDictionary.ContainsKey(player.Username))
            throw new Exception("Player is not in the room");

        var claims = new Claim[]
        {
            new("username", player.Username),
            new("roomCode", room.Code)
        };

        var identity = new ClaimsIdentity(claims);

        return room.JwtService.GenerateToken(identity, now => now.AddYears(1));
    }
    
    public static string GetTokenForPlayer(this Models.Room room, string username)
    {
        var player = room.GetPlayer(username);
        
        return room.GetTokenForPlayer(player);
    }
    
}