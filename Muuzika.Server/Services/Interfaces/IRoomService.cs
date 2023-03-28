using Muuzika.Server.Dtos.Gateway;

namespace Muuzika.Server.Services.Interfaces;

public interface IRoomService
{
    RoomJoinedDto CreateRoom(string nickname);
    RoomJoinedDto JoinRoom(string roomCode, string nickname);
}