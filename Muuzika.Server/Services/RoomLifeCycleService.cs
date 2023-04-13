using Muuzika.Server.Models;
using Muuzika.Server.Providers.Interfaces;
using Muuzika.Server.Services.Interfaces;
using ILogger = Serilog.ILogger;

namespace Muuzika.Server.Services;

public class RoomLifeCycleService: IRoomLifeCycleService
{

    private readonly Room _room;
    private readonly IRoomHubService _hubService;
    private readonly IRoomWorkerService _workerService;
    private readonly IConfigProvider _configProvider;
    private readonly ILogger _logger;
    
    public RoomLifeCycleService(Room room, IRoomHubService hubService, IRoomWorkerService workerService, IConfigProvider configProvider, ILogger logger)
    {
        _room = room;
        _hubService = hubService;
        _workerService = workerService;
        _configProvider = configProvider;
        _logger = logger;
    }
    
    public void SetOptions(RoomOptions options)
    {
        _room.Options = options;
        _logger.Information("Room options set to {@Options}", options);
        _hubService.SendToAll(client => client.RoomOptionsChanged(options));
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