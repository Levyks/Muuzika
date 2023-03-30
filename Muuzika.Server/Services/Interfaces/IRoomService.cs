using Muuzika.Server.Dtos.Gateway;

namespace Muuzika.Server.Services.Interfaces;

public interface IRoomService
{
    Task<RoomCreatedOrJoinedDto> CreateRoom(CreateOrJoinRoomDto createRoomDto);
    Task<RoomCreatedOrJoinedDto> JoinRoom(string roomCode, CreateOrJoinRoomDto joinRoomDto);
}