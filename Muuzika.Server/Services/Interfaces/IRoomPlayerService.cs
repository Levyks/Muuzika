using System.Collections.Immutable;
using Muuzika.Server.Models;

namespace Muuzika.Server.Services.Interfaces;

public interface IRoomPlayerService
{
    IEnumerable<Player> GetPlayers();
    IEnumerable<Player> GetConnectedPlayers();
    bool HasConnectedPlayers();
    Player? FindPlayer(string username);
    Player GetPlayer(string username);
    Player AddPlayer(string username);
    void RemovePlayer(Player player);
    void KickPlayer(string username);
    void SetLeader(Player player);
    string GetTokenForPlayer(Player player);

    void HandlePlayerConnection(Player player);
    void HandlePlayerDisconnection(Player player);
    void SchedulePlayerRemoval(Player player);
}