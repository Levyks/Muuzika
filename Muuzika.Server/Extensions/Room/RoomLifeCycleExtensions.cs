namespace Muuzika.Server.Extensions.Room;

public static class RoomLifeCycleExtensions
{
    public static void CloseIfEmpty(this Models.Room room)
    {
        if (room.GetConnectedPlayers().Any()) return;

        room.Logger.Information("Room is empty, closing");
        room.Dispose();
    }
    
    public static void ScheduleCloseIfEmpty(this Models.Room room)
    {
        var delay = room.ConfigProvider.DelayBeforeRoomCloseIfEmpty;
     
        room.Logger.Information("Room will be closed in {Delay} minutes if there's no player connected", delay.TotalMinutes);

        room.CloseIfEmptyCancellationTokenSource = room.ScheduleTask(_ =>
        {
            room.CloseIfEmptyCancellationTokenSource = null;
            room.CloseIfEmpty();
        }, delay);
    }
    
    public static void CancelCloseIfEmptyScheduleIfSet(this Models.Room room)
    {
        if (room.CloseIfEmptyCancellationTokenSource == null) return;
        
        room.Logger.Information("Canceling scheduled room close");
        room.CancellationTokenSources.Remove(room.CloseIfEmptyCancellationTokenSource);
        room.CloseIfEmptyCancellationTokenSource.Cancel();
        room.CloseIfEmptyCancellationTokenSource.Dispose();
        room.CloseIfEmptyCancellationTokenSource = null;
    }
}