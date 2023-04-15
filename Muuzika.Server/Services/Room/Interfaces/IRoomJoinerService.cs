using Muuzika.Server.Dtos.Gateway;

namespace Muuzika.Server.Services.Room.Interfaces;

public interface IRoomJoinerService
{
    Task<RoomCreatedOrJoinedDto> CreateRoom(CreateOrJoinRoomDto createRoomDto);
    Task<RoomCreatedOrJoinedDto> JoinRoom(string roomCode, CreateOrJoinRoomDto joinRoomDto);
}