using Muuzika.Server.Models;

namespace Muuzika.Server.Extensions.Room;

public static class RoomPlayerLifeCycleExtensions
{
    public static void HandlePlayerConnection(this Models.Room room, Player player)
    {
        room.Logger.Information("Player {Username} connected", player.Username);
        room.CancelPlayerRemovalScheduleIfSet(player);
        room.WatchTask(room.ToAllExcept(player).PlayerIsConnectedChanged(player.Username, player.IsConnected));
    }
    
    public static void HandlePlayerDisconnection(this Models.Room room, Player player)
    {
        room.Logger.Information("Player {Username} disconnected", player.Username);
        room.WatchTask(room.ToAllExcept(player).PlayerIsConnectedChanged(player.Username, player.IsConnected));
        room.SchedulePlayerRemoval(player);
    }

    public static void SchedulePlayerRemoval(this Models.Room room, Player player)
    {        
        var delay = room.ConfigProvider.DelayDisconnectedPlayerRemoval;
     
        room.Logger.Information("Player {Player} will be removed in {Delay} minutes if he does not reconnects", player.Username, delay.TotalMinutes);

        player.DisconnectedPlayerRemovalCancellationTokenSource = room.ScheduleTask(_ =>
        {
            player.DisconnectedPlayerRemovalCancellationTokenSource = null;
            room.RemovePlayer(player);
        }, delay);
    }
    
    private static void CancelPlayerRemovalScheduleIfSet(this Models.Room room, Player player)
    {
        if (player.DisconnectedPlayerRemovalCancellationTokenSource == null) return;
        
        room.Logger.Information("Canceling scheduled player removal");
        player.DisconnectedPlayerRemovalCancellationTokenSource.Cancel();
        room.CancellationTokenSources.Remove(player.DisconnectedPlayerRemovalCancellationTokenSource);
        player.DisconnectedPlayerRemovalCancellationTokenSource.Dispose();
        player.DisconnectedPlayerRemovalCancellationTokenSource = null;
    }
    
    public static void SetOptions(this Models.Room room, RoomOptions options)
    {
        room.Options = options;
        room.Logger.Information("Room options set to {@Options}", options);
        room.WatchTask(room.ToAll().RoomOptionsChanged(options));
    }
    
}