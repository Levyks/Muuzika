namespace Muuzika.Server.Providers.Interfaces;

public interface IConfigProvider
{
    string JwtKey { get; }
    string JwtIssuer { get; }
    string JwtAudience { get; }
    
    TimeSpan DelayCloseRoomAfterLastPlayerLeft { get; }
    TimeSpan DelayDisconnectedPlayerRemoval { get; }
}