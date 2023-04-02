using Muuzika.Server.Models;

namespace Muuzika.Server.Repositories.Interfaces;

public interface IRoomRepository
{
    Room? FindRoomByCode(string code);
    Room GetRoomByCode(string code);
    string? PopAvailableCode();
    void PushAvailableCode(string code);
    void StoreRoom(Room room);
    bool RemoveRoom(Room room);
}