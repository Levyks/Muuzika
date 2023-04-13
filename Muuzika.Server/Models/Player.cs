using Microsoft.AspNetCore.SignalR;
using Muuzika.Server.Hubs;

namespace Muuzika.Server.Models;

public class Player
{
    public Room Room { get; set; }
    public string Username { get; set; }
    public int Score { get; set; }
    public HubCallerContext? HubContext { get; set; }
    public bool IsConnected => HubContext != null;
    
    public bool IsLeader => Room.Leader == this;
    
    public CancellationTokenSource? DisconnectedPlayerRemovalCancellationTokenSource { get; set; }
    
    public Player(Room room, string username)
    {
        Room = room;
        Username = username;
    }
}