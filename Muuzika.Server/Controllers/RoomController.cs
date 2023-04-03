using Microsoft.AspNetCore.Mvc;
using Muuzika.Server.Dtos.Gateway;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;
    
    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpPost(Name = "/")]
    public Task<RoomCreatedOrJoinedDto> CreateRoom([FromBody] CreateOrJoinRoomDto createRoomDto)
    {
        return _roomService.CreateRoom(createRoomDto);
    }
    
    [HttpPost("{roomCode}")]
    public Task<RoomCreatedOrJoinedDto> JoinRoom([FromRoute] string roomCode, [FromBody] CreateOrJoinRoomDto joinRoomDto)
    {
        Console.WriteLine($"Joining room {roomCode}");
        return _roomService.JoinRoom(roomCode, joinRoomDto);
    }
}