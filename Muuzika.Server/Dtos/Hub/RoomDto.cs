using Muuzika.Server.Enums.Room;

namespace Muuzika.Server.Dtos.Hub;

public record RoomDto(
    string Code,
    string LeaderUsername,
    RoomStatus Status,
    RoomPossibleRoundTypes PossibleRoundTypes,
    IEnumerable<PlayerDto> Players
);