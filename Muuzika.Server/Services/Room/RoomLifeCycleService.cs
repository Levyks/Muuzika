using Muuzika.Server.Dtos.Hub.Responses;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Models;
using Muuzika.Server.Models.Interfaces;
using Muuzika.Server.Providers.Interfaces;
using Muuzika.Server.Services.Room.Interfaces;
using ILogger = Serilog.ILogger;

namespace Muuzika.Server.Services.Room;

public class RoomLifeCycleService: IRoomLifeCycleService
{

    private readonly Models.Room _room;
    private readonly IRoomHubService _hubService;
    private readonly IRoomWorkerService _workerService;
    private readonly IConfigProvider _configProvider;
    private readonly IPlaylistMapper _playlistMapper;
    private readonly ILogger _logger;
    
    public RoomLifeCycleService(Models.Room room, IRoomHubService hubService, IRoomWorkerService workerService, IConfigProvider configProvider, IPlaylistMapper playlistMapper, ILogger logger)
    {
        _room = room;
        _hubService = hubService;
        _workerService = workerService;
        _configProvider = configProvider;
        _playlistMapper = playlistMapper;
        _logger = logger;
    }
    
    public void SetOptions(RoomOptions options)
    {
        _room.Options = options;
        _logger.Information("Room options set to {@Options}", options);
        _hubService.SendToAll(client => client.OptionsChanged(options));
    }
    
    public PlaylistDto SetPlaylist(IPlaylist playlist, bool alsoNotifyLeader = true)
    {
        _room.Playlist = playlist;
        var playlistDto = _playlistMapper.ToDto(playlist);
        _logger.Information("Room playlist set to {@Playlist}", playlistDto);
        
        if (alsoNotifyLeader)
            _hubService.SendToAll(client => client.PlaylistChanged(playlistDto));
        else
            _hubService.SendToAllExcept(_room.Leader, client => client.PlaylistChanged(playlistDto));
        
        return playlistDto;
    }
    
    private void CloseIfEmpty()
    {
        if (_room.PlayersDictionary.Count > 0) return;

        _logger.Information("Room is empty, closing");
        _room.Dispose();
    }
    
    public void ScheduleCloseIfEmpty()
    {
        var delay = _configProvider.DelayCloseRoomAfterLastPlayerLeft;
     
        _logger.Information("Room will be closed in {Delay} minutes if there's no player connected", delay.TotalMinutes);


        _room.CloseAfterLastPlayerLeftCancellationTokenSource = _workerService.ScheduleTask(_ =>
        {
            _room.CloseAfterLastPlayerLeftCancellationTokenSource = null;
            CloseIfEmpty();
            return ValueTask.CompletedTask;
        }, delay);
    }
    
    public void CancelCloseIfEmptySchedule()
    {
        if (_room.CloseAfterLastPlayerLeftCancellationTokenSource == null) return;
        
        _logger.Information("Canceling scheduled room close");
        _room.CloseAfterLastPlayerLeftCancellationTokenSource.Cancel();
        _room.CancellationTokenSources.Remove(_room.CloseAfterLastPlayerLeftCancellationTokenSource);
        _room.CloseAfterLastPlayerLeftCancellationTokenSource.Dispose();
        _room.CloseAfterLastPlayerLeftCancellationTokenSource = null;
    }
}