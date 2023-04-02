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
    
    private readonly IRoomMapper _roomMapper;
    private readonly IExceptionMapper _exceptionMapper;
    
    private Player Player => Context.Items["player"] as Player ?? throw new Exception("Player not found in hub context");
    private Room Room => Player.Room;
    
    public RoomHub(IJwtService jwtService, IRoomRepository roomRepository, IRoomMapper roomMapper, IExceptionMapper exceptionMapper)
    {
        JwtService = jwtService;
        RoomRepository = roomRepository;
        _roomMapper = roomMapper;
        _exceptionMapper = exceptionMapper;
    }
    
    public InvocationResultDto<StateSyncDto> SyncAll() => WrapInvocation(() => _roomMapper.ToStateSyncDto(Room, Player));

    public InvocationResultDto<object?> LeaveRoom() => WrapInvocation<object?>(() =>
    {
        Room.RemovePlayer(Player);
        Context.Abort();
        return null;
    });
    
    #region Lifecycle
    public override async Task OnConnectedAsync()
    {
        var player = this.ParseTokenAndGetPlayer();
        
        player.ConnectionId = Context.ConnectionId;
        Context.Items.Add("player", player);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, Room.Code);
        Room.HandlePlayerConnection(player);

        await base.OnConnectedAsync();
    }
    
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Player.ConnectionId = null;
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
    #endregion
}