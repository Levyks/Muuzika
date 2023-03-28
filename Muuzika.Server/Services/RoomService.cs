using Muuzika.Server.Dtos.Gateway;
using Muuzika.Server.Models;
using Muuzika.Server.Models.Extensions.Room;
using Muuzika.Server.Repositories.Interfaces;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Services;

public class RoomService: IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IJwtService _jwtService;
    
    public RoomService(IRoomRepository roomRepository, IJwtService jwtService)
    {
        _roomRepository = roomRepository;
        _jwtService = jwtService;
    }
    
    public RoomJoinedDto CreateRoom(string username)
    {
        var code = _roomRepository.FindAvailableRoomCode();
        
        if (code == null)
        {
            throw new Exception("No available room codes");
        }
        
        var room = new Room(code, username, _jwtService);

        return new RoomJoinedDto
        (
            Username: room.Leader.Username,
            RoomCode: room.Code,
            Token: room.GetTokenForPlayer(room.Leader)
        );
    }

    public RoomJoinedDto JoinRoom(string roomCode, string nickname)
    {
        throw new NotImplementedException();
    }
}