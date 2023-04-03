using System.Net;
using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Exceptions;

public class InvalidCaptchaException : BaseException
{
    public override ExceptionType Type => ExceptionType.InvalidCaptcha;
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    public override string Message => "Invalid captcha";
}