using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Exceptions;

public class OutOfAvailableRoomCodesException : BaseException
{
    public override ExceptionType Type => ExceptionType.OutOfAvailableRoomCodes;
    public override string Message => "Out of available room codes";
}