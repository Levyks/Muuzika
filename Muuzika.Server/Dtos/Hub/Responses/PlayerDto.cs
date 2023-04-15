namespace Muuzika.Server.Dtos.Hub.Responses;

public record PlayerDto(
    string Username,
    int Score,
    bool IsConnected
);