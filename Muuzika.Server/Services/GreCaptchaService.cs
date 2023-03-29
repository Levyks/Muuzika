using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Services;

public class GreCaptchaService: ICaptchaService
{
    public Task<bool> ValidateCaptchaAsync(CaptchaAction action, string token)
    {
        throw new NotImplementedException();
    }
}