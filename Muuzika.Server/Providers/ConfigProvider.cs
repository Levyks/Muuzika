using Muuzika.Server.Enums.Room;
using Muuzika.Server.Providers.Interfaces;

namespace Muuzika.Server.Providers;

public class ConfigProvider: IConfigProvider
{
    private readonly IConfiguration _configuration;
    
    public string JwtKey { get; }
    public string JwtIssuer { get; }
    public string JwtAudience { get; }
    public TimeSpan DelayCloseRoomAfterLastPlayerLeft { get; }
    public TimeSpan DelayDisconnectedPlayerRemoval { get; }
    
    public RoomPossibleRoundTypes RoomDefaultPossibleRoundTypes { get; }
    public ushort RoomDefaultRoundsCount { get; }
    public TimeSpan RoomDefaultRoundDuration { get; }
    public ushort RoomDefaultMaxPlayersCount { get; }
    
    public ConfigProvider(IConfiguration configuration)
    {
        _configuration = configuration;

        JwtKey = Get("Jwt:Key");
        JwtIssuer = Get("Jwt:Issuer");
        JwtAudience = Get("Jwt:Audience");
        DelayCloseRoomAfterLastPlayerLeft = GetTimeSpan("Room:DelayCloseRoomAfterLastPlayerLeft");
        DelayDisconnectedPlayerRemoval = GetTimeSpan("Room:DelayDisconnectedPlayerRemoval");
        RoomDefaultPossibleRoundTypes = GetEnum<RoomPossibleRoundTypes>("Room:DefaultPossibleRoundTypes");
        RoomDefaultRoundsCount = GetUshort("Room:DefaultRoundsCount");
        RoomDefaultRoundDuration = GetTimeSpan("Room:DefaultRoundDuration");
        RoomDefaultMaxPlayersCount = GetUshort("Room:DefaultMaxPlayersCount");
    }
    
    private T GetEnum<T>(string key) where T: Enum
    {
        try
        {
            return (T)Enum.Parse(typeof(T), Get(key));
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"{key} is not a valid {typeof(T).Name}", ex);
        }
    }

    private ushort GetUshort(string key)
    {
        try
        {
            return ushort.Parse(Get(key));
        }
        catch (FormatException ex)
        {
            throw new FormatException($"{key} is not a valid ushort", ex);
        }
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