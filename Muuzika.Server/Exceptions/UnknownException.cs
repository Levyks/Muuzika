using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Exceptions;

public class UnknownException: BaseException
{
    public override ExceptionType Type => ExceptionType.Unknown;
    public override string Message => "Unknown error";
}