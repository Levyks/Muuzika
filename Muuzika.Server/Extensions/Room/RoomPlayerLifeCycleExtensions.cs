namespace Muuzika.Server.Extensions.Room;

public static class RoomPlayerLifeCycleExtensions
{
    public static void HandlePlayerConnection(this Models.Room room, Models.Player player)
    {
        room.Logger.Information("Player {Username} connected", player.Username);
        room.CancelCloseIfEmptyScheduleIfSet();
        room.WatchTask(room.ToAllExcept(player).PlayerJoined(player.Username));
    }
    
    public static void HandlePlayerDisconnection(this Models.Room room, Models.Player player)
    {
        room.Logger.Information("Player {Username} disconnected", player.Username);
        
        if (!room.HasConnectedPlayers())
            room.ScheduleCloseIfEmpty();
    }
    
}