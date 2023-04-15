using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Muuzika.Server.Attributes.Auth;
using Muuzika.Server.Dtos.Hub.Requests;
using Muuzika.Server.Dtos.Hub.Responses;
using Muuzika.Server.Exceptions;
using Muuzika.Server.Hubs.Interfaces;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Models;
using Muuzika.Server.Services.Playlist.Interfaces;
using Muuzika.Server.Services.Room.Interfaces;

namespace Muuzika.Server.Hubs;

[Authorize]
public class RoomHub: Hub<IRoomHubClient>
{
    private readonly IGenericPlaylistFetcherService _genericPlaylistFetcherService;
    private readonly IRoomMapper _roomMapper;
    private readonly IExceptionMapper _exceptionMapper;
    private readonly IServiceProvider _serviceProvider;
    
    private Player Player => Context.Items["player"] as Player ?? throw new Exception("Player not found in hub context");
    private Room Room => Player.Room;
    
    private IRoomPlayerService? _playerService;
    private IRoomPlayerService PlayerService => _playerService ??= Room.ServiceProvider.GetRequiredService<IRoomPlayerService>();

    private IRoomLifeCycleService? _lifeCycleService;
    private IRoomLifeCycleService LifeCycleService => _lifeCycleService ??= Room.ServiceProvider.GetRequiredService<IRoomLifeCycleService>();
    
    public RoomHub(IGenericPlaylistFetcherService genericPlaylistFetcherService, IRoomMapper roomMapper, IExceptionMapper exceptionMapper, IServiceProvider serviceProvider)
    {
        _genericPlaylistFetcherService = genericPlaylistFetcherService;
        _roomMapper = roomMapper;
        _exceptionMapper = exceptionMapper;
        _serviceProvider = serviceProvider;
    }
    
    public StateSyncDto SyncAll() => _roomMapper.ToStateSyncDto(Room, Player);

    public void LeaveRoom() 
    {
        PlayerService.RemovePlayer(Player);
        Context.Abort();
    }
    
    private void Validate<T>(T obj, int position = 0) where T : class
    {
        if (obj == null) throw new InvalidArgumentsException(new ValidationException($"Argument at position {position} must not be null."));
        
        try
        {
            var validationContext = new ValidationContext(obj, _serviceProvider, null);
            Validator.ValidateObject(obj, validationContext, true);
        }
        catch (ValidationException ex)
        {
            throw new InvalidArgumentsException(ex);
        }
    }
    
    [LeaderOnly]
    public void SetOptions(RoomOptions options) 
    {
        Validate(options);
        LifeCycleService.SetOptions(options);
    }
    
    [LeaderOnly]
    public async Task<PlaylistDto> SetPlaylist(SetPlaylistDto setPlaylistDto) 
    {
        Validate(setPlaylistDto);
        var playlist = await _genericPlaylistFetcherService.FetchPlaylistAsync(setPlaylistDto.Provider, setPlaylistDto.Id);
        return LifeCycleService.SetPlaylist(playlist, false);
    }
    
    [LeaderOnly]
    public void KickPlayer(string username) 
    {
        PlayerService.KickPlayer(username);
    }


    #region Lifecycle
    public override async Task OnConnectedAsync()
    {
        try
        {
            object? playerObj = null;
            Context.GetHttpContext()?.Items.TryGetValue("player", out playerObj);
            if (playerObj is not Player player)
                throw new UnknownException();

            player.HubContext = Context;
            Context.Items.Add("player", player);

            await Groups.AddToGroupAsync(Context.ConnectionId, Room.Code);
            PlayerService.HandlePlayerConnection(player);

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            throw _exceptionMapper.ToHubException(ex);
        }
    }
    
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (Player.HubContext != null)
        {
            Player.HubContext = null;
            PlayerService.HandlePlayerDisconnection(Player);
        }

        return base.OnDisconnectedAsync(exception);
    }
    #endregion
}