using Microsoft.AspNetCore.SignalR;
using Muuzika.Server.Hubs;
using Muuzika.Server.Hubs.Interfaces;
using Muuzika.Server.Models;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Services;

public class RoomHubService: IRoomHubService
{
    private readonly Room _room;
    private readonly IRoomWorkerService _workerService;
    private readonly IHubContext<RoomHub, IRoomHubClient> _hubContext;
    
    public RoomHubService(Room room, IRoomWorkerService workerService, IHubContext<RoomHub, IRoomHubClient> hubContext)
    {
        _room = room;
        _workerService = workerService;
        _hubContext = hubContext;
    }

    public IRoomHubClient? To(Player player)
    {
        return player.HubContext == null ? null : _hubContext.Clients.Client(player.HubContext.ConnectionId);
    }

    public IRoomHubClient ToAll()
    {
        return _hubContext.Clients.Group(_room.Code);
    }

    public IRoomHubClient ToAllExcept(Player player)
    {
        return player.HubContext == null ? ToAll() : _hubContext.Clients.GroupExcept(_room.Code, new [] { player.HubContext.ConnectionId });
    }

    public void SendTo(Player player, Func<IRoomHubClient, Task> func)
    {
        var client = To(player);
        if (client != null)
        {
            _workerService.WatchTask(func(client));
        }
    }

    public void SendToAll(Func<IRoomHubClient, Task> func)
    {
        var client = ToAll();
        _workerService.WatchTask(func(client));
    }

    public void SendToAllExcept(Player player, Func<IRoomHubClient, Task> func)
    {
        var client = ToAllExcept(player);
        _workerService.WatchTask(func(client));
    }

    public void DisconnectPlayer(Player player)
    {
        player.HubContext?.Abort();
        player.HubContext = null;
    }
}