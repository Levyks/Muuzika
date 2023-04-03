using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Exceptions;

public class LeaderOnlyActionException: BaseException
{
    public override ExceptionType Type => ExceptionType.LeaderOnlyAction;
    public override string Message => "Only leader can perform this action";
}