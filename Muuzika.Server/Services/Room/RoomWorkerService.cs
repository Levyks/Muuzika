using Muuzika.Server.Services.Room.Interfaces;

namespace Muuzika.Server.Services.Room;

public class RoomWorkerService: IRoomWorkerService
{

    private readonly Models.Room _room;
    private readonly Serilog.ILogger _logger;
    
    public RoomWorkerService(Models.Room room, Serilog.ILogger logger)
    {
        _room = room;
        _logger = logger;
    }
    
    private void HandleBackgroundException(Exception exception)
    {
        _logger.Error(exception, "Task failed");
    }
    
    public void WatchTask(Task? task)
    {
        task?.ContinueWith(t =>
        {
            if (t.Exception != null)
                HandleBackgroundException(t.Exception);
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    public CancellationTokenSource ScheduleTask(Func<CancellationTokenSource, ValueTask> func, TimeSpan delay)
    {
        var cts = new CancellationTokenSource();

        var task = Task.Delay(delay, cts.Token);
        
        _room.CancellationTokenSources.Add(cts);
        
        WatchTask(task);
            
        // ReSharper disable once MethodSupportsCancellation
        task.ContinueWith(t =>
        {
            _room.CancellationTokenSources.Remove(cts);
            if (!t.IsCompletedSuccessfully) return;

            try
            {
                var result = func(cts);
                if (!result.IsCompleted) WatchTask(result.AsTask());
            } 
            catch (Exception ex)
            {
                HandleBackgroundException(ex);
            }
        });
        
        return cts;
    }

    public void CancelAllTasks()
    {
        lock (_room.CancellationTokenSources)
        {
            foreach (var cancellationTokenSource in _room.CancellationTokenSources)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }
            _room.CancellationTokenSources.Clear();
        }
    }
}