using Muuzika.Server.Enums.Room;
using Muuzika.Server.Providers.Interfaces;

namespace Muuzika.Server.Models;

public class RoomOptions
{
    public RoomPossibleRoundTypes PossibleRoundTypes { get; set; }
    public ushort RoundsCount { get; set; }
    public TimeSpan RoundDuration { get; set; }
    public ushort MaxPlayersCount { get; set; }
    
    public RoomOptions(RoomPossibleRoundTypes possibleRoundTypes, ushort roundsCount, TimeSpan roundDuration, ushort maxPlayersCount)
    {
        PossibleRoundTypes = possibleRoundTypes;
        RoundsCount = roundsCount;
        RoundDuration = roundDuration;
        MaxPlayersCount = maxPlayersCount;
    }

    public static RoomOptions Default(IConfigProvider configProvider) => new(
        configProvider.RoomDefaultPossibleRoundTypes,
        configProvider.RoomDefaultRoundsCount,
        configProvider.RoomDefaultRoundDuration,
        configProvider.RoomDefaultMaxPlayersCount);
}