using Muuzika.Server.Hubs.Interfaces;

namespace Muuzika.Server.Extensions.Room;

public static class RoomHubExtensions
{
    public static IRoomHubClient? To(this Models.Room room, Models.Player player)
    {
        return player.HubContext == null ? null : room.HubContext.Clients.Client(player.HubContext.ConnectionId);
    }
    public static IRoomHubClient ToAll(this Models.Room room)
    {
        return room.HubContext.Clients.Group(room.Code);
    }
    
    public static IRoomHubClient ToAllExcept(this Models.Room room, Models.Player player)
    {
        return player.HubContext == null ? room.ToAll() : room.HubContext.Clients.GroupExcept(room.Code, new [] { player.HubContext.ConnectionId });
    }
    
    public static void DisconnectPlayer(this Models.Room room, Models.Player player)
    {
        player.HubContext?.Abort();
        player.HubContext = null;
    }
    
}