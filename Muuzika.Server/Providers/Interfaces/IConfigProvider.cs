using Muuzika.Server.Enums.Room;

namespace Muuzika.Server.Providers.Interfaces;

public interface IConfigProvider
{
    string JwtKey { get; }
    string JwtIssuer { get; }
    string JwtAudience { get; }
    
    TimeSpan DelayCloseRoomAfterLastPlayerLeft { get; }
    TimeSpan DelayDisconnectedPlayerRemoval { get; }
    
    RoomPossibleRoundTypes RoomDefaultPossibleRoundTypes { get; }
    int RoomDefaultRoundCount { get; }
    TimeSpan RoomDefaultRoundDuration { get; }
    int RoomDefaultMaxPlayersCount { get; }
    
    int RoomMinRoundsCount { get; }
    int RoomMaxRoundsCount { get; }
    
    int RoomMinMaxPlayersCount { get; }
    int RoomMaxMaxPlayersCount { get; }
    
    TimeSpan RoomMinRoundDuration { get; }
    TimeSpan RoomMaxRoundDuration { get; }
    
    string SpotifyClientId { get; }
    string SpotifyClientSecret { get; }
}