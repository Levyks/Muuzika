using Muuzika.Server.Dtos.Hub;
using Muuzika.Server.Dtos.Hub.Responses;
using Muuzika.Server.Models;

namespace Muuzika.Server.Hubs.Interfaces;

public interface IRoomHubClient
{
    Task SyncAll(StateSyncDto stateSync);
    
    Task PlayerJoined(string username);
    Task PlayerKicked(string username);
    Task PlayerLeft(string username);
    Task PlayerIsConnectedChanged(string username, bool isConnected);
    Task LeaderChanged(string username);
    Task OptionsChanged(RoomOptions options);
    Task PlaylistChanged(PlaylistDto playlist);
    
    Task UpdateScores(Dictionary<string, int> scores);
    
}