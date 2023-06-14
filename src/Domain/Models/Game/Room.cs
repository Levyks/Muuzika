using Muuzika.Domain.Enums;

namespace Muuzika.Domain.Models;

public class Room
{
    public RoomStatus Status { get; set; } = RoomStatus.Lobby;
    public RoomOptions Options { get; set; }

    public ISet<Player> Players = new HashSet<Player>();
    
    

    public Room(RoomOptions options)
    {
        Options = options;
    }
}