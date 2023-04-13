using Muuzika.Server.Models;

namespace Muuzika.Server.Services.Interfaces;

public interface IRoomWorkerService
{
    void WatchTask(Task? task);
    CancellationTokenSource ScheduleTask(Func<CancellationTokenSource, ValueTask> func, TimeSpan delay); 
    void CancelAllTasks();
}