namespace Muuzika.Server.Dtos.Gateway;

public record RoomCreatedOrJoinedDto(
    string Username, 
    string RoomCode, 
    string Token
    );