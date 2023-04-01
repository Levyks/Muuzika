namespace Muuzika.Server.Models.Extensions.Room;

public static class RoomTaskExtensions
{
    
    private static void HandleTaskException(this Models.Room room, Exception? exception)
    {
        if (exception == null) return;
        
        
    }
    
    public static CancellationTokenSource ScheduleTask(this Models.Room room, Action action, int delayMs)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        
        Task.Delay(delayMs, cancellationTokenSource.Token)
            .ContinueWith(task =>
            {
                if (task.IsCanceled) return;
                if (task.IsFaulted) room.HandleTaskException(task.Exception);
                if (task.IsCompleted) action();
            }, cancellationTokenSource.Token);
        
        return cancellationTokenSource;
    }
    
}