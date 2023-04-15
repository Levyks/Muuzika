namespace Muuzika.Server.Services.Room.Interfaces;

public interface IRoomWorkerService
{
    void WatchTask(Task? task);
    CancellationTokenSource ScheduleTask(Func<CancellationTokenSource, ValueTask> func, TimeSpan delay); 
    void CancelAllTasks();
}