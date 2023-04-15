using System.Security.Claims;
using Muuzika.Server.Exceptions;
using Muuzika.Server.Extensions;
using Muuzika.Server.Models;
using Muuzika.Server.Providers.Interfaces;
using Muuzika.Server.Services.Interfaces;
using Muuzika.Server.Services.Room.Interfaces;
using ILogger = Serilog.ILogger;

namespace Muuzika.Server.Services.Room;

public class RoomPlayerService: IRoomPlayerService
{
    private readonly Models.Room _room;
    private readonly IRoomHubService _hubService;
    private readonly IRoomWorkerService _workerService;
    private readonly IRoomLifeCycleService _lifeCycleService;
    private readonly IJwtService _jwtService;
    private readonly IRandomProvider _randomProvider;
    private readonly IConfigProvider _configProvider;
    private readonly ILogger _logger;
    
    
    public RoomPlayerService(Models.Room room, IRoomHubService hubService, IRoomWorkerService workerService, IRoomLifeCycleService lifeCycleService, IJwtService jwtService, IRandomProvider randomProvider, IConfigProvider configProvider, ILogger logger)
    {
        _room = room;
        _hubService = hubService;
        _workerService = workerService;
        _lifeCycleService = lifeCycleService;
        _jwtService = jwtService;
        _randomProvider = randomProvider;
        _configProvider = configProvider;
        _logger = logger;
    }

    public IEnumerable<Player> GetPlayers()
    {
        return _room.PlayersDictionary.Values;
    }

    public IEnumerable<Player> GetConnectedPlayers()
    {
        return GetPlayers().Where(p => p.IsConnected);
    }

    public bool HasConnectedPlayers()
    {
        return GetConnectedPlayers().Any();
    }

    public Player? FindPlayer(string username)
    {
        return _room.PlayersDictionary.TryGetValue(username, out var player) ? player : null;
    }

    public Player GetPlayer(string username)
    {
        return FindPlayer(username) ?? throw new PlayerNotFoundException(_room.Code, username);
    }
    
    private Player AddPlayer(Player player)
    {
        if (_room.CloseAfterLastPlayerLeftCancellationTokenSource != null)
            _lifeCycleService.CancelCloseIfEmptySchedule();
        
        _room.PlayersDictionary.Add(player.Username, player);
        _logger.Information("Player {Username} joined the room", player.Username);
        _hubService.SendToAll(client => client.PlayerJoined(player.Username));
        
        SchedulePlayerRemoval(player);
        return player;
    }

    public Player AddPlayer(string username)
    {
        var player = new Player(_room, username);
        return AddPlayer(player);
    }

    public void RemovePlayer(Player player)
    {
        _room.PlayersDictionary.Remove(player.Username);
        _logger.Information("Player {Username} left the room", player.Username);
        _hubService.SendToAll(client => client.PlayerLeft(player.Username));
        
        _hubService.DisconnectPlayer(player);

        if (_room.PlayersDictionary.Count == 0)
        {
            _lifeCycleService.ScheduleCloseIfEmpty();
            return;
        }
        
        if (player.IsLeader)
        {
            SetLeaderToRandomPlayer();
        }
    }
    
    private void KickPlayer(Player player)
    {
        if (player.IsLeader)
            throw new CannotKickLeaderException();
        
        RemovePlayer(player);
        _hubService.SendToAll(client => client.PlayerKicked(player.Username));
    }

    public void KickPlayer(string username)
    {
        var player = GetPlayer(username);
        KickPlayer(player);
    }
    
    private void SetLeaderToRandomPlayer()
    {
        var random = _randomProvider.GetRandom();
        var player = HasConnectedPlayers() ? 
            GetConnectedPlayers().PickRandom(random) : 
            GetPlayers().PickRandom(random);
        SetLeader(player);
    }

    public void SetLeader(Player player)
    {
        _room.Leader = player;
        _hubService.SendToAll(client => client.LeaderChanged(player.Username));
    }

    public string GetTokenForPlayer(Player player)
    {
        if (!_room.PlayersDictionary.ContainsKey(player.Username))
            throw new Exception("Player is not in the room");

        var claims = new Claim[]
        {
            new("username", player.Username),
            new("roomCode", _room.Code)
        };

        var identity = new ClaimsIdentity(claims);

        return _jwtService.GenerateToken(identity, now => now.AddYears(1));
    }

    public void HandlePlayerConnection(Player player)
    {
        _logger.Information("Player {Username} connected", player.Username);
        CancelPlayerRemovalScheduleIfSet(player);
        _hubService.SendToAllExcept(player, client => client.PlayerIsConnectedChanged(player.Username, player.IsConnected));
    }
    
    private void CancelPlayerRemovalScheduleIfSet(Player player)
    {
        if (player.DisconnectedPlayerRemovalCancellationTokenSource == null) return;
        
        _logger.Information("Canceling scheduled player removal");
        player.DisconnectedPlayerRemovalCancellationTokenSource.Cancel();
        _room.CancellationTokenSources.Remove(player.DisconnectedPlayerRemovalCancellationTokenSource);
        player.DisconnectedPlayerRemovalCancellationTokenSource.Dispose();
        player.DisconnectedPlayerRemovalCancellationTokenSource = null;
    }

    public void HandlePlayerDisconnection(Player player)
    {
        _logger.Information("Player {Username} disconnected", player.Username);
        _hubService.SendToAllExcept(player, client => client.PlayerIsConnectedChanged(player.Username, player.IsConnected));
        SchedulePlayerRemoval(player);
    }

    public void SchedulePlayerRemoval(Player player)
    {
        var delay = _configProvider.DelayDisconnectedPlayerRemoval;
     
        _logger.Information("Player {Player} will be removed in {Delay} minutes if he does not reconnects", player.Username, delay.TotalMinutes);

        player.DisconnectedPlayerRemovalCancellationTokenSource = _workerService.ScheduleTask(_ =>
        {
            player.DisconnectedPlayerRemovalCancellationTokenSource = null;
            RemovePlayer(player); 
            return ValueTask.CompletedTask;
        }, delay);
    }
}