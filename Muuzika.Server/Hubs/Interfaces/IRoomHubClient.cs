using Muuzika.Server.Dtos.Hub;
using Muuzika.Server.Models;

namespace Muuzika.Server.Hubs.Interfaces;

public interface IRoomHubClient
{
    Task SyncAll(StateSyncDto stateSync);
    
    Task PlayerJoined(string username);
    Task PlayerKicked(string username);
    Task PlayerLeft(string username);
    Task PlayerIsConnectedChanged(string username, bool isConnected);
    Task RoomLeaderChanged(string username);
    Task RoomOptionsChanged(RoomOptions options);
    
    Task UpdateScores(Dictionary<string, int> scores);
    
}