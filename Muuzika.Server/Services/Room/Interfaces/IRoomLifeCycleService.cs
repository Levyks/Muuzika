using Muuzika.Server.Dtos.Hub.Responses;
using Muuzika.Server.Models;
using Muuzika.Server.Models.Interfaces;

namespace Muuzika.Server.Services.Room.Interfaces;

public interface IRoomLifeCycleService
{
    void SetOptions(RoomOptions options);
    PlaylistDto SetPlaylist(IPlaylist playlist, bool alsoNotifyLeader = true);
    void ScheduleCloseIfEmpty();
    void CancelCloseIfEmptySchedule();
}