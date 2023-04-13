using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Muuzika.Server.Attributes.Auth;
using Muuzika.Server.Dtos.Hub;
using Muuzika.Server.Exceptions;
using Muuzika.Server.Hubs.Interfaces;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Models;
using Muuzika.Server.Repositories.Interfaces;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Hubs;

[Authorize]
public class RoomHub: Hub<IRoomHubClient>
{
    public readonly IJwtService JwtService;
    public readonly IRoomRepository RoomRepository;
    public readonly JsonSerializerOptions JsonSerializerOptions;
    
    private readonly IRoomMapper _roomMapper;
    private readonly IExceptionMapper _exceptionMapper;
    
    private Player Player => Context.Items["player"] as Player ?? throw new Exception("Player not found in hub context");
    private Room Room => Player.Room;
    
    private IRoomPlayerService? _playerService;
    private IRoomPlayerService PlayerService => _playerService ??= Room.ServiceProvider.GetRequiredService<IRoomPlayerService>();

    private IRoomLifeCycleService? _lifeCycleService;
    private IRoomLifeCycleService LifeCycleService => _lifeCycleService ??= Room.ServiceProvider.GetRequiredService<IRoomLifeCycleService>();
    
    public RoomHub(IJwtService jwtService, IRoomRepository roomRepository, IRoomMapper roomMapper, IExceptionMapper exceptionMapper, JsonSerializerOptions jsonSerializerOptions)
    {
        JwtService = jwtService;
        RoomRepository = roomRepository;
        _roomMapper = roomMapper;
        _exceptionMapper = exceptionMapper;
        JsonSerializerOptions = jsonSerializerOptions;
    }
    
    public StateSyncDto SyncAll() => _roomMapper.ToStateSyncDto(Room, Player);

    public void LeaveRoom() 
    {
        PlayerService.RemovePlayer(Player);
        Context.Abort();
    }
    
    [LeaderOnly]
    public void SetOptions(RoomOptions options) 
    {
        LifeCycleService.SetOptions(options);
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
        if (Player.HubContext == null) return base.OnDisconnectedAsync(exception);
        
        Player.HubContext = null;
        PlayerService.HandlePlayerDisconnection(Player);
        
        return base.OnDisconnectedAsync(exception);
    }
    #endregion
}