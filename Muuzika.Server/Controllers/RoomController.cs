using Microsoft.AspNetCore.Mvc;
using Muuzika.Server.Dtos.Gateway;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly IPlaylistFetcherService _playlistFetcherService;
    
    public RoomController(IRoomService roomService, IPlaylistFetcherService playlistFetcherService)
    {
        _roomService = roomService;
        _playlistFetcherService = playlistFetcherService;
    }

    [HttpPost]
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
    
    [HttpGet("playlist/{playlistId}")]
    public async Task GetPlaylistInfo([FromRoute] string playlistId)
    {
        var playlist = await _playlistFetcherService.FetchPlaylistAsync(playlistId);
        Console.WriteLine(playlist);
    }
}