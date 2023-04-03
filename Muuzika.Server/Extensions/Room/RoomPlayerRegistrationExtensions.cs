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
    
    public static Player AddPlayer(this Models.Room room, Player player)
    {
        if (room.CloseAfterLastPlayerLeftCancellationTokenSource != null)
            room.CancelCloseIfEmptySchedule();
        
        room.PlayersDictionary.Add(player.Username, player);
        room.Logger.Information("Player {Username} joined the room", player.Username);
        room.WatchTask(room.ToAll().PlayerJoined(player.Username));
        
        room.SchedulePlayerRemoval(player);
        return player;
    }
    
    public static Player AddPlayer(this Models.Room room, string username)
    {
        var player = new Player(room, username);
        return room.AddPlayer(player);
    }

    public static void RemovePlayer(this Models.Room room, Player player)
    {
        room.PlayersDictionary.Remove(player.Username);
        room.Logger.Information("Player {Username} left the room", player.Username);
        room.WatchTask(room.ToAll().PlayerLeft(player.Username));
        
        room.DisconnectPlayer(player);

        if (room.PlayersDictionary.Count == 0)
        {
            room.ScheduleCloseIfEmpty();
        }
        else if (room.Leader == player)
        {
            room.SetLeader(room.GetRandomPlayer());
        }
    }
    
    public static void KickPlayer(this Models.Room room, Player player)
    {
        if (room.Leader == player)
            throw new CannotKickLeaderException();
        
        room.RemovePlayer(player);
        room.WatchTask(room.ToAll().PlayerKicked(player.Username));
    }
    
    public static void KickPlayer(this Models.Room room, string username)
    {
        var player = room.GetPlayer(username);
        room.KickPlayer(player);
    }
    
    public static Player GetRandomPlayer(this Models.Room room)
    {
        var random = room.RandomProvider.GetRandom();
        return room.PlayersDictionary.Values.ElementAt(random.Next(room.PlayersDictionary.Count));
    }
    
    public static void SetLeader(this Models.Room room, Player player)
    {
        room.Leader = player;
        room.WatchTask(room.ToAll().RoomLeaderChanged(player.Username));
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