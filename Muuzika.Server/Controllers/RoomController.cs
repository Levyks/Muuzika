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
    public RoomJoinedDto CreateRoom([FromBody] UsernameDto usernameDto)
    {
        return _roomService.CreateRoom(usernameDto.Username);
    }
    
    [HttpPost("/{roomCode}")]
    public RoomJoinedDto JoinRoom([FromRoute] string roomCode, [FromBody] UsernameDto usernameDto)
    {
        return _roomService.JoinRoom(roomCode, usernameDto.Username);
    }
}