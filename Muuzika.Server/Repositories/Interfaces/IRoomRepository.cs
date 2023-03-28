using Muuzika.Server.Models;

namespace Muuzika.Server.Repositories.Interfaces;

public interface IRoomRepository
{
    Room? FindRoomByCode(string code);
    string? FindAvailableRoomCode();
}