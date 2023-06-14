using Muuzika.Domain.Enums;

namespace Muuzika.Domain.Models;

public record RoomOptions(
    RoomPossibleRoundTypes PossibleRoundTypes,
    int RoundCount,
    TimeSpan RoundDuration,
    int MaxPlayers
);