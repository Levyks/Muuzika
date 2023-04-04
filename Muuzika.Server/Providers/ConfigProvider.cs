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
    
    public ushort RoomMinRoundsCount { get; }
    public ushort RoomMaxRoundsCount { get; }
    
    public ushort RoomMinMaxPlayersCount { get; }
    public ushort RoomMaxMaxPlayersCount { get; }
    
    public TimeSpan RoomMinRoundDuration { get; }
    public TimeSpan RoomMaxRoundDuration { get; }
    
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
        
        RoomMinRoundsCount = GetUshort("Room:MinRoundsCount");
        RoomMaxRoundsCount = GetUshort("Room:MaxRoundsCount");
        
        RoomMinMaxPlayersCount = GetUshort("Room:MinMaxPlayersCount");
        RoomMaxMaxPlayersCount = GetUshort("Room:MaxMaxPlayersCount");
        
        RoomMinRoundDuration = GetTimeSpan("Room:MinRoundDuration");
        RoomMaxRoundDuration = GetTimeSpan("Room:MaxRoundDuration");
    }
    
    private T GetEnum<T>(string key) where T: Enum
    {
        var value = Get(key);
        try
        {
            return (T)Enum.Parse(typeof(T), value);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"{key} is not a valid value for {typeof(T).Name} (value: \"{value}\")", ex);
        }
    }

    private ushort GetUshort(string key)
    {
        var value = Get(key);
        try
        {
            return ushort.Parse(value);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException($"{key} is not a valid ushort (value: \"{value}\")", ex);
        }
    }

    private TimeSpan GetTimeSpan(string key)
    {
        var value = Get(key);
        try
        {
            return TimeSpan.Parse(value);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException($"{key} is not a valid TimeSpan (value: \"{value}\")", ex);
        }
    }
    
    private string Get(string key)
    {
        return _configuration[key] ?? throw new ArgumentNullException(key, $"{key} is not set in configuration");
    }
}