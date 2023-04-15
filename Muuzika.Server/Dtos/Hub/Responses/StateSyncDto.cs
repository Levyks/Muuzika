namespace Muuzika.Server.Dtos.Hub.Responses;

public record StateSyncDto(
    RoomDto Room,
    PlayerDto Player
);