namespace Muuzika.Server.Extensions.Room;

public static class RoomLifeCycleExtensions
{
    private static void CloseIfEmpty(this Models.Room room)
    {
        if (room.PlayersDictionary.Count > 0) return;

        room.Logger.Information("Room is empty, closing");
        room.Dispose();
    }
    
    public static void ScheduleCloseIfEmpty(this Models.Room room)
    {
        var delay = room.ConfigProvider.DelayCloseRoomAfterLastPlayerLeft;
     
        room.Logger.Information("Room will be closed in {Delay} minutes if there's no player connected", delay.TotalMinutes);

        room.CloseAfterLastPlayerLeftCancellationTokenSource = room.ScheduleTask(_ =>
        {
            room.CloseAfterLastPlayerLeftCancellationTokenSource = null;
            room.CloseIfEmpty();
        }, delay);
    }
    
    public static void CancelCloseIfEmptySchedule(this Models.Room room)
    {
        if (room.CloseAfterLastPlayerLeftCancellationTokenSource == null) return;
        
        room.Logger.Information("Canceling scheduled room close");
        room.CloseAfterLastPlayerLeftCancellationTokenSource.Cancel();
        room.CancellationTokenSources.Remove(room.CloseAfterLastPlayerLeftCancellationTokenSource);
        room.CloseAfterLastPlayerLeftCancellationTokenSource.Dispose();
        room.CloseAfterLastPlayerLeftCancellationTokenSource = null;
    }
}