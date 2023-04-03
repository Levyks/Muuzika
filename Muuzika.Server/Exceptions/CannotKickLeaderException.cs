using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Exceptions;

public class CannotKickLeaderException: BaseException
{
    public override ExceptionType Type => ExceptionType.CannotKickLeader;
    public override string Message => "Cannot kick the leader";
}