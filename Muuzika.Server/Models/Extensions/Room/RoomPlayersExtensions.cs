using System.Security.Claims;

namespace Muuzika.Server.Models.Extensions.Room;

public static class RoomPlayersExtensions
{
    public static Player? GetPlayer(this Models.Room room, string username)
    {
        return room.PlayersDictionary.TryGetValue(username, out var player) ? player : null;
    }
    
    public static bool RemovePlayer(this Models.Room room, string username)
    {
        return room.PlayersDictionary.Remove(username);
    }
    
    public static Player AddPlayer(this Models.Room room, Player player)
    {
        room.PlayersDictionary.Add(player.Username, player);
        return player;
    }
    
    public static Player AddPlayer(this Models.Room room, string username)
    {
        var player = new Player(room, username);
        return room.AddPlayer(player);
    }

    public static string GetTokenForPlayer(this Models.Room room, Player player)
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
        if (player == null)
            throw new Exception("Player is not in the room");
        
        return room.GetTokenForPlayer(player);
    }
}