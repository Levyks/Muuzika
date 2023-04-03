using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using Muuzika.Server.Dtos.Hub;
using Muuzika.Server.Exceptions;
using Muuzika.Server.Extensions.Room;
using Muuzika.Server.Hubs.Extensions;
using Muuzika.Server.Hubs.Interfaces;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Models;
using Muuzika.Server.Repositories.Interfaces;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Hubs;

public class RoomHub: Hub<IRoomHubClient>
{
    public readonly IJwtService JwtService;
    public readonly IRoomRepository RoomRepository;
    public readonly JsonSerializerOptions JsonSerializerOptions;
    
    private readonly IRoomMapper _roomMapper;
    private readonly IExceptionMapper _exceptionMapper;
    
    private Player Player => Context.Items["player"] as Player ?? throw new Exception("Player not found in hub context");
    private Room Room => Player.Room;
    
    public RoomHub(IJwtService jwtService, IRoomRepository roomRepository, IRoomMapper roomMapper, IExceptionMapper exceptionMapper, JsonSerializerOptions jsonSerializerOptions)
    {
        JwtService = jwtService;
        RoomRepository = roomRepository;
        _roomMapper = roomMapper;
        _exceptionMapper = exceptionMapper;
        JsonSerializerOptions = jsonSerializerOptions;
    }
    
    public InvocationResultDto<StateSyncDto> SyncAll() => WrapInvocation(() => _roomMapper.ToStateSyncDto(Room, Player));

    public InvocationResultDto<object?> LeaveRoom() => WrapInvocation<object?>(() =>
    {
        Room.RemovePlayer(Player);
        Context.Abort();
        return null;
    });
    
    public InvocationResultDto<object?> SetOptions(RoomOptions options) => WrapLeaderOnlyInvocation<object?>(() =>
    {
        Room.SetOptions(options);
        return null;
    });
    
    public InvocationResultDto<object?> KickPlayer(string username) => WrapLeaderOnlyInvocation<object?>(() =>
    {
        Room.KickPlayer(username);
        return null;
    });


    #region Lifecycle
    public override async Task OnConnectedAsync()
    {
        try
        {
            var player = this.ParseTokenAndGetPlayer();

            player.HubContext = Context;
            Context.Items.Add("player", player);

            await Groups.AddToGroupAsync(Context.ConnectionId, Room.Code);
            Room.HandlePlayerConnection(player);

            await base.OnConnectedAsync();
        }
        catch (BaseException ex)
        {
            var stringifiedEx = JsonSerializer.Serialize(_exceptionMapper.ToDto(ex), JsonSerializerOptions);
            throw new HubException($"$@{stringifiedEx}");
        }
    }
    
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (Player.HubContext == null) return base.OnDisconnectedAsync(exception);
        
        Player.HubContext = null;
        Room.HandlePlayerDisconnection(Player);
        
        return base.OnDisconnectedAsync(exception);
    }
    #endregion

    #region Wrappers
    private InvocationResultDto<T> WrapInvocation<T>(Func<T> func)
    {
        try
        {
            return new InvocationResultDto<T>(true, func(), default);
        } 
        catch (Exception ex)
        {
            var baseException = ex as BaseException ?? new UnknownException();

            if (baseException is UnknownException && Context.Items["room"] is Room room)
                room.Logger.Error(ex, "Unknown exception in hub invocation");

            return new InvocationResultDto<T>(false, default, _exceptionMapper.ToDto(baseException));
        }
    }
    
    private async Task<InvocationResultDto<T>> WrapInvocationAsync<T>(Func<Task<T>> func)
    {
        try
        {
            return new InvocationResultDto<T>(true, await func(), default);
        } 
        catch (Exception ex)
        {
            var baseException = ex as BaseException ?? new UnknownException();
            
            if (baseException is UnknownException && Context.Items["room"] is Room room)
                room.Logger.Error(ex, "Unknown exception in hub invocation");
            
            return new InvocationResultDto<T>(false, default, _exceptionMapper.ToDto(baseException));
        }
    }
    
    private InvocationResultDto<T> WrapLeaderOnlyInvocation<T>(Func<T> func) => WrapInvocation(() =>
    {
        if (Room.Leader != Player) throw new LeaderOnlyActionException();
        return func();
    });
    
    private async Task<InvocationResultDto<T>> WrapLeaderOnlyInvocationAsync<T>(Func<Task<T>> func) => await WrapInvocationAsync(() =>
    {
        if (Room.Leader != Player) throw new LeaderOnlyActionException();
        return func();
    });
    #endregion
}