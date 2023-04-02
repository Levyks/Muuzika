using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Exceptions;

public class NoTokenProvidedException : BaseException
{
    public override ExceptionType Type => ExceptionType.NoTokenProvided;
    public override string Message => "No token provided";
}