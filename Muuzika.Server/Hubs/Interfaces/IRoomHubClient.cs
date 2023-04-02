using Muuzika.Server.Dtos.Hub;
using Muuzika.Server.Exceptions;

namespace Muuzika.Server.Hubs.Interfaces;

public interface IRoomHubClient
{
    Task SyncAll(StateSyncDto stateSync);
    
    Task PlayerJoined(string username);
    Task PlayerLeft(string username);
    Task PlayerIsConnectedChanged(string username, bool isConnected);
    
    Task UpdateScores(Dictionary<string, int> scores);
}