using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.IdentityModel.Tokens;
using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Exceptions;

public class InvalidArgumentsException: BaseException
{
    public override ExceptionType Type => ExceptionType.InvalidArguments;
    public override HttpStatusCode StatusCode => HttpStatusCode.BadGateway;

    public override string Message
    {
        get
        {
            const string message = "Invalid arguments";
            if (!_validationResult.MemberNames.Any()) return message;
            return message + $": {string.Join(", ", _validationResult.MemberNames)}";
        }
    }

    private readonly ValidationResult _validationResult;
    
    public InvalidArgumentsException(ValidationException exception): base(exception.ValidationResult)
    {
        _validationResult = exception.ValidationResult;
    }
}