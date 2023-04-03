using System.Net;
using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Exceptions;

public class CannotKickLeaderException: BaseException
{
    public override ExceptionType Type => ExceptionType.CannotKickLeader;
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    public override string Message => "Cannot kick the leader";
}