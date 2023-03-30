using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Exceptions;

public class InvalidCaptchaException : BaseException
{
    public override ExceptionType Type => ExceptionType.InvalidCaptcha;
    public override string Message => "Invalid captcha";
}