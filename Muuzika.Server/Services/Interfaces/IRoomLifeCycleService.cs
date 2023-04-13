using Muuzika.Server.Models;

namespace Muuzika.Server.Services.Interfaces;

public interface IRoomLifeCycleService
{
    void SetOptions(RoomOptions options);
    void ScheduleCloseIfEmpty();
    void CancelCloseIfEmptySchedule();
}