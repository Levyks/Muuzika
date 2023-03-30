using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Services;

public class NoOpCaptchaService: ICaptchaService
{
    public Task<bool> ValidateCaptchaAsync(CaptchaAction action, string token)
    {
        return Task.FromResult(true);
    }
}