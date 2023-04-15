using Muuzika.Server.Hubs.Interfaces;
using Muuzika.Server.Models;

namespace Muuzika.Server.Services.Room.Interfaces;

public interface IRoomHubService
{
    IRoomHubClient? To(Player player);
    IRoomHubClient ToAll();
    IRoomHubClient ToAllExcept(Player player);
    
    void SendTo(Player player, Func<IRoomHubClient, Task> func);
    void SendToAll(Func<IRoomHubClient, Task> func);
    void SendToAllExcept(Player player, Func<IRoomHubClient, Task> func);
    
    void DisconnectPlayer(Player player);
}