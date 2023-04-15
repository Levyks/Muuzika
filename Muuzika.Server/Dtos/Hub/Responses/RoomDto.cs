using Muuzika.Server.Enums.Room;
using Muuzika.Server.Models;

namespace Muuzika.Server.Dtos.Hub.Responses;

public record RoomDto(
    string Code,
    string LeaderUsername,
    RoomStatus Status,
    IEnumerable<PlayerDto> Players,
    RoomOptions Options
);