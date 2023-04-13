using System.Collections.Immutable;
using Microsoft.AspNetCore.SignalR;
using Muuzika.Server.Enums.Room;
using Muuzika.Server.Extensions;
using Muuzika.Server.Hubs;
using Muuzika.Server.Hubs.Interfaces;
using Muuzika.Server.Providers.Interfaces;
using Muuzika.Server.Repositories.Interfaces;
using Muuzika.Server.Services;
using Muuzika.Server.Services.Interfaces;
using ILogger = Serilog.ILogger;

namespace Muuzika.Server.Models;

public sealed class Room: IDisposable
{    
    public readonly string Code;
    public Player Leader { get; set; }
    public RoomStatus Status { get; set; } = RoomStatus.InLobby;
    public RoomOptions Options { get; set; }
    public ImmutableArray<Round>? Rounds { get; set; }
    
    internal readonly Dictionary<string, Player> PlayersDictionary = new();

    private readonly IRoomRepository _repository;
    private readonly ILogger _logger;
    private readonly IRoomWorkerService _workerService;
    
    internal readonly IServiceProvider ServiceProvider;
    
    public CancellationTokenSource? CloseAfterLastPlayerLeftCancellationTokenSource { get; set; }
    public readonly HashSet<CancellationTokenSource> CancellationTokenSources = new();
    
    
    public Room(string code, string leaderUsername, IRoomRepository repository, IServiceProvider parentServiceProvider)
    {
        Code = code;
        ServiceProvider = CreateServiceProvider(parentServiceProvider);
        Options = RoomOptions.Default(ServiceProvider.GetRequiredService<IConfigProvider>());
        
        _logger = ServiceProvider.GetRequiredService<ILogger>();
        _workerService = ServiceProvider.GetRequiredService<IRoomWorkerService>();
        _repository = repository;
        
        Leader = ServiceProvider.GetRequiredService<IRoomPlayerService>().AddPlayer(leaderUsername);
    }
    
    private ServiceProvider CreateServiceProvider(IServiceProvider parentServiceProvider)
    {
        var logger = parentServiceProvider.GetRequiredService<ILogger>()
            .ForContext<Room>()
            .ForContext("AdditionalSourceIdentifier", Code);
        
        var serviceCollection = new ServiceCollection();
        
        serviceCollection.Reuse<IJwtService>(parentServiceProvider);
        serviceCollection.Reuse<IRandomProvider>(parentServiceProvider);
        serviceCollection.Reuse<IConfigProvider>(parentServiceProvider);
        serviceCollection.Reuse<IHubContext<RoomHub, IRoomHubClient>>(parentServiceProvider);
        
        serviceCollection.AddSingleton(this);
        serviceCollection.AddSingleton(logger);
        serviceCollection.AddSingleton<IRoomWorkerService, RoomWorkerService>();
        serviceCollection.AddSingleton<IRoomHubService, RoomHubService>();
        serviceCollection.AddSingleton<IRoomLifeCycleService, RoomLifeCycleService>();
        serviceCollection.AddSingleton<IRoomPlayerService, RoomPlayerService>();
        
        return serviceCollection.BuildServiceProvider();
    }
    
    ~Room()
    {
        _logger.Information("Garbage collected");
    }
    
    public void Dispose()
    {
        _logger.Information("Disposing");
        _repository.RemoveRoom(this);
        _workerService.CancelAllTasks();
        if (ServiceProvider is IDisposable disposable)
            disposable.Dispose();
    }
}