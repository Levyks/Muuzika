using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Services.Interfaces;

public interface ICaptchaService
{
    Task<bool> ValidateCaptchaAsync(CaptchaAction action, string token);
}