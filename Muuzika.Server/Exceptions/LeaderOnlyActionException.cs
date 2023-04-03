using System.Net;
using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Exceptions;

public class LeaderOnlyActionException: BaseException
{
    public override ExceptionType Type => ExceptionType.LeaderOnlyAction;
    public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
    public override string Message => "Only leader can perform this action";
}