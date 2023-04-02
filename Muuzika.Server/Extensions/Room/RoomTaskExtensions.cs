namespace Muuzika.Server.Extensions.Room;

public static class RoomTaskExtensions
{
    
    private static void HandleBackgroundException(this Models.Room room, Exception exception)
    {
        room.Logger.Error(exception, "Task failed");
    }
    
    public static void WatchTask(this Models.Room room, Task? task)
    {
        task?.ContinueWith(t =>
        {
            if (t.Exception != null)
                room.HandleBackgroundException(t.Exception);
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    public static CancellationTokenSource ScheduleTask(this Models.Room room, Func<CancellationTokenSource, Task> asyncFunc, TimeSpan delay)
    {
        var cts = new CancellationTokenSource();

        var task = Task.Delay(delay, cts.Token);
        
        room.CancellationTokenSources.Add(cts);
        
        room.WatchTask(task);
            
        // ReSharper disable once MethodSupportsCancellation
        task.ContinueWith(t =>
        {
            room.CancellationTokenSources.Remove(cts);
            if (t.IsCompletedSuccessfully)
                room.WatchTask(asyncFunc(cts));
        });
        
        return cts;
    }
    
    public static CancellationTokenSource ScheduleTask(this Models.Room room, Action<CancellationTokenSource> action, TimeSpan delay)
    {
        var cts = new CancellationTokenSource();

        var task = Task.Delay(delay, cts.Token);
        
        room.CancellationTokenSources.Add(cts);
        
        room.WatchTask(task);
            
        // ReSharper disable once MethodSupportsCancellation
        task.ContinueWith(t =>
        {
            room.CancellationTokenSources.Remove(cts);
            if (!t.IsCompletedSuccessfully) return;

            try
            {
                action(cts);
            }
            catch (Exception e)
            {
                room.HandleBackgroundException(e);
            }
        });
        
        return cts;
    }
    
    public static void CancelAllTasks(this Models.Room room)
    {
        lock (room.CancellationTokenSources)
        {
            foreach (var cancellationTokenSource in room.CancellationTokenSources)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }
            room.CancellationTokenSources.Clear();
        }
    }
}