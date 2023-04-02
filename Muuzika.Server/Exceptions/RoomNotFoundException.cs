using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Exceptions;

public class RoomNotFoundException: BaseException
{
    public override ExceptionType Type => ExceptionType.RoomNotFound;
    private readonly string _roomCode;
    public override string Message => $"Room with code {_roomCode} not found";
    public RoomNotFoundException(string roomCode): base(new { roomCode })
    {
        _roomCode = roomCode;
    }
}