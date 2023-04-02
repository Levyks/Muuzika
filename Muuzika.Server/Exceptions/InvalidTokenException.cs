using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Exceptions;

public class InvalidTokenException : BaseException
{
    public override ExceptionType Type => ExceptionType.InvalidToken;
    public override string Message => "Invalid token";
}