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
    public int RoomDefaultRoundCount { get; }
    public TimeSpan RoomDefaultRoundDuration { get; }
    public int RoomDefaultMaxPlayersCount { get; }
    
    public int RoomMinRoundsCount { get; }
    public int RoomMaxRoundsCount { get; }
    
    public int RoomMinMaxPlayersCount { get; }
    public int RoomMaxMaxPlayersCount { get; }
    
    public TimeSpan RoomMinRoundDuration { get; }
    public TimeSpan RoomMaxRoundDuration { get; }
    
    public string SpotifyClientId { get; }
    public string SpotifyClientSecret { get; }
    
    public ConfigProvider(IConfiguration configuration)
    {
        _configuration = configuration;

        JwtKey = Get("Jwt:Key");
        JwtIssuer = Get("Jwt:Issuer");
        JwtAudience = Get("Jwt:Audience");
        
        DelayCloseRoomAfterLastPlayerLeft = GetTimeSpan("Room:DelayCloseRoomAfterLastPlayerLeft");
        DelayDisconnectedPlayerRemoval = GetTimeSpan("Room:DelayDisconnectedPlayerRemoval");
        
        RoomDefaultPossibleRoundTypes = GetEnum<RoomPossibleRoundTypes>("Room:DefaultPossibleRoundTypes");
        RoomDefaultRoundCount = GetInt("Room:DefaultRoundCount");
        RoomDefaultRoundDuration = GetTimeSpan("Room:DefaultRoundDuration");
        RoomDefaultMaxPlayersCount = GetInt("Room:DefaultMaxPlayersCount");
        
        RoomMinRoundsCount = GetInt("Room:MinRoundsCount");
        RoomMaxRoundsCount = GetInt("Room:MaxRoundsCount");
        
        RoomMinMaxPlayersCount = GetInt("Room:MinMaxPlayersCount");
        RoomMaxMaxPlayersCount = GetInt("Room:MaxMaxPlayersCount");
        
        RoomMinRoundDuration = GetTimeSpan("Room:MinRoundDuration");
        RoomMaxRoundDuration = GetTimeSpan("Room:MaxRoundDuration");
        
        SpotifyClientId = Get("Spotify:ClientId");
        SpotifyClientSecret = Get("Spotify:ClientSecret");
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

    private int GetInt(string key)
    {
        var value = Get(key);
        try
        {
            return int.Parse(value);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException($"{key} is not a valid int (value: \"{value}\")", ex);
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
    
    private string? GetOptional(string key)
    {
        return _configuration[key];
    }
    
    private string Get(string key)
    {
        return GetOptional(key) ?? throw new ArgumentNullException(key, $"{key} is not set in configuration");
    }
}