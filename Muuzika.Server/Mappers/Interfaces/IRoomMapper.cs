using Muuzika.Server.Dtos.Gateway;
using Muuzika.Server.Dtos.Hub;
using Muuzika.Server.Models;

namespace Muuzika.Server.Mappers.Interfaces;

public interface IRoomMapper
{
    RoomDto ToDto(Room room);
    RoomCreatedOrJoinedDto ToCreatedOrJoinedDto(Room room, Player player);
    StateSyncDto ToStateSyncDto(Room room, Player player);
}