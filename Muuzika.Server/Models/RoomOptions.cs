using System.ComponentModel.DataAnnotations;
using Muuzika.Server.Attributes.Validation;
using Muuzika.Server.Enums.Room;
using Muuzika.Server.Providers.Interfaces;

namespace Muuzika.Server.Models;

public class RoomOptions
{
    public RoomPossibleRoundTypes PossibleRoundTypes { get; }
    
    [RangeFromConfig(nameof(IConfigProvider.RoomMinRoundsCount), nameof(IConfigProvider.RoomMaxRoundsCount))]
    public ushort RoundsCount { get; }
    
    [TimeSpanRangeFromConfig(nameof(IConfigProvider.RoomMinRoundDuration), nameof(IConfigProvider.RoomMaxRoundDuration))]
    public TimeSpan RoundDuration { get; }
    
    [RangeFromConfig(nameof(IConfigProvider.RoomMinRoundsCount), nameof(IConfigProvider.RoomMaxRoundsCount))]
    public ushort MaxPlayersCount { get; }
    
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