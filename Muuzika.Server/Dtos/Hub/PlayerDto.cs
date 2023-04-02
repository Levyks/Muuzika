namespace Muuzika.Server.Dtos.Hub;

public record PlayerDto(
    string Username,
    int Score,
    bool IsConnected
);