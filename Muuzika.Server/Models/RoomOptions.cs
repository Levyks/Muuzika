using Muuzika.Server.Attributes.Validation;
using Muuzika.Server.Enums.Room;
using Muuzika.Server.Providers.Interfaces;

namespace Muuzika.Server.Models;

public class RoomOptions
{
    public RoomPossibleRoundTypes PossibleRoundTypes { get; }
    
    [RangeFromConfig(nameof(IConfigProvider.RoomMinRoundsCount), nameof(IConfigProvider.RoomMaxRoundsCount))]
    public int RoundCount { get; }
    
    [TimeSpanRangeFromConfig(nameof(IConfigProvider.RoomMinRoundDuration), nameof(IConfigProvider.RoomMaxRoundDuration))]
    public TimeSpan RoundDuration { get; }
    
    [RangeFromConfig(nameof(IConfigProvider.RoomMinMaxPlayersCount), nameof(IConfigProvider.RoomMaxMaxPlayersCount))]
    public int MaxPlayersCount { get; }
    
    public RoomOptions(RoomPossibleRoundTypes possibleRoundTypes, int roundCount, TimeSpan roundDuration, int maxPlayersCount)
    {
        PossibleRoundTypes = possibleRoundTypes;
        RoundCount = roundCount;
        RoundDuration = roundDuration;
        MaxPlayersCount = maxPlayersCount;
    }

    public static RoomOptions Default(IConfigProvider configProvider) => new(
        configProvider.RoomDefaultPossibleRoundTypes,
        configProvider.RoomDefaultRoundCount,
        configProvider.RoomDefaultRoundDuration,
        configProvider.RoomDefaultMaxPlayersCount);
}