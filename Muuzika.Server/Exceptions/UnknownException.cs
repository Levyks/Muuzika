using System.Net;
using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Exceptions;

public class UnknownException: BaseException
{
    public override ExceptionType Type => ExceptionType.Unknown;
    public override HttpStatusCode StatusCode => HttpStatusCode.InternalServerError;
    public override string Message => "Unknown error";
}