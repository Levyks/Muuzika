using Muuzika.Server.Providers.Interfaces;

namespace Muuzika.Server.Providers;

public class ConfigProvider: IConfigProvider
{
    private readonly IConfiguration _configuration;
    
    public string JwtKey { get; }
    public string JwtIssuer { get; }
    public string JwtAudience { get; }
    public TimeSpan DelayBeforeRoomCloseIfEmpty { get; }

    public ConfigProvider(IConfiguration configuration)
    {
        _configuration = configuration;

        JwtKey = Get("Jwt:Key");
        JwtIssuer = Get("Jwt:Issuer");
        JwtAudience = Get("Jwt:Audience");
        DelayBeforeRoomCloseIfEmpty = GetTimeSpan("Room:DelayBeforeCloseIfEmpty");
    }

    private TimeSpan GetTimeSpan(string key)
    {
        try
        {
            return TimeSpan.Parse(Get(key));
        }
        catch (FormatException ex)
        {
            throw new FormatException($"{key} is not a valid TimeSpan", ex);
        }
    }
    
    private string Get(string key)
    {
        return _configuration[key] ?? throw new ArgumentNullException(key, $"{key} is not set in configuration");
    }
    
    private string Get(string key, string defaultValue)
    {
        return _configuration[key] ?? defaultValue;
    }
}