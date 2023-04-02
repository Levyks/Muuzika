using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Exceptions;

public class PlayerNotFoundException: BaseException
{
    public override ExceptionType Type => ExceptionType.PlayerNotFound;
    
    private readonly string _roomCode;
    private readonly string _username;
    
    public override string Message => $"Player {_username} not found in room {_roomCode}";

    public PlayerNotFoundException(string roomCode, string username): base(new { roomCode, username })
    {
        _roomCode = roomCode;
        _username = username;
    }
}