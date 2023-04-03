using System.Collections.Immutable;
using Microsoft.AspNetCore.SignalR;
using Muuzika.Server.Enums.Room;
using Muuzika.Server.Extensions.Room;
using Muuzika.Server.Hubs;
using Muuzika.Server.Hubs.Interfaces;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Providers.Interfaces;
using Muuzika.Server.Repositories.Interfaces;
using Muuzika.Server.Services.Interfaces;
using Serilog;

namespace Muuzika.Server.Models;

public sealed class Room: IDisposable
{    
    public readonly string Code;
    public Player Leader { get; set; }
    public RoomStatus Status { get; set; } = RoomStatus.InLobby;
    public RoomOptions Options { get; set; }
    public ImmutableArray<Round>? Rounds { get; set; }
    
    internal readonly Dictionary<string, Player> PlayersDictionary = new();

    internal readonly Serilog.ILogger Logger;
    internal readonly IJwtService JwtService;
    internal readonly IConfigProvider ConfigProvider;
    internal readonly IRandomProvider RandomProvider;
    internal readonly IHubContext<RoomHub, IRoomHubClient> HubContext;
    internal readonly IRoomMapper RoomMapper;
    
    private readonly IRoomRepository _repository;
    
    public CancellationTokenSource? CloseAfterLastPlayerLeftCancellationTokenSource { get; set; }
    public readonly HashSet<CancellationTokenSource> CancellationTokenSources = new();
    
    
    public Room(string code, string leaderUsername, IRoomRepository repository, IServiceProvider serviceProvider)
    {
        Logger = Log.Logger
            .ForContext<Room>()
            .ForContext("AdditionalSourceIdentifier", code);

        JwtService = serviceProvider.GetRequiredService<IJwtService>();
        HubContext = serviceProvider.GetRequiredService<IHubContext<RoomHub, IRoomHubClient>>();
        ConfigProvider = serviceProvider.GetRequiredService<IConfigProvider>();
        RandomProvider = serviceProvider.GetRequiredService<IRandomProvider>();
        RoomMapper = serviceProvider.GetRequiredService<IRoomMapper>();

        _repository = repository;
        
        Options = RoomOptions.Default(ConfigProvider);
        
        Code = code;
        Leader = this.AddPlayer(leaderUsername);
        
        this.ScheduleCloseIfEmpty();
    }
    
    ~Room()
    {
        Logger.Information("Garbage collected");
    }
    
    public void Dispose()
    {
        Logger.Information("Disposing");
        _repository.RemoveRoom(this);
        this.CancelAllTasks();
    }
}