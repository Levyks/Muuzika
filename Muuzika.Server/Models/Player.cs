namespace Muuzika.Server.Models;

public class Player
{
    public Room Room { get; set; }
    public string Username { get; set; }
    
    public Player(Room room, string username)
    {
        Room = room;
        Username = username;
    }
}