using System.Net;
using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Exceptions;

public class InvalidTokenException : BaseException
{
    public override ExceptionType Type => ExceptionType.InvalidToken;
    public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
    public override string Message => "Invalid token";
}