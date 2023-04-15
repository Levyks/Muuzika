using Microsoft.AspNetCore.Mvc;
using Muuzika.Server.Dtos.Gateway;
using Muuzika.Server.Services.Interfaces;
using Muuzika.Server.Services.Playlist.Interfaces;
using Muuzika.Server.Services.Room.Interfaces;

namespace Muuzika.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class RoomController : ControllerBase
{
    private readonly IRoomJoinerService _roomJoinerService;
    private readonly IPlaylistFetcherService _playlistFetcherService;
    
    public RoomController(IRoomJoinerService roomJoinerService, IPlaylistFetcherService playlistFetcherService)
    {
        _roomJoinerService = roomJoinerService;
        _playlistFetcherService = playlistFetcherService;
    }

    [HttpPost]
    public Task<RoomCreatedOrJoinedDto> CreateRoom([FromBody] CreateOrJoinRoomDto createRoomDto)
    {
        return _roomJoinerService.CreateRoom(createRoomDto);
    }
    
    [HttpPost("{roomCode}")]
    public Task<RoomCreatedOrJoinedDto> JoinRoom([FromRoute] string roomCode, [FromBody] CreateOrJoinRoomDto joinRoomDto)
    {
        Console.WriteLine($"Joining room {roomCode}");
        return _roomJoinerService.JoinRoom(roomCode, joinRoomDto);
    }
    
    [HttpGet("playlist/{playlistId}")]
    public async Task GetPlaylistInfo([FromRoute] string playlistId)
    {
        var playlist = await _playlistFetcherService.FetchPlaylistAsync(playlistId);
        Console.WriteLine(playlist);
    }
}