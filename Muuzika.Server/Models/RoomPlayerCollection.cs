using System.Collections.Immutable;

namespace Muuzika.Server.Models;

public class RoomPlayerCollection
{
    private readonly Room _room;
    private readonly Dictionary<string, Player> _players = new();

    public ImmutableList<Player> All => _players.Values.ToImmutableList();
    public ImmutableList<Player> Connected => _players.Values.Where(p => p.IsConnected).ToImmutableList();

    public RoomPlayerCollection(Room room)
    {
        _room = room;
    }
    
    public Player Add(string username)
    {
        var player = new Player(_room, username);
        _players.Add(username, player);
        return player;
    }
    
    public Player? Get(string username)
    {
        return _players.TryGetValue(username, out var player) ? player : null;
    }
}