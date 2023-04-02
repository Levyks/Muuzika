namespace Muuzika.Server.Dtos.Hub;

public record StateSyncDto(
    RoomDto Room,
    PlayerDto Player
);