using System.Collections.Immutable;
using Muuzika.Server.Enums.Room;
using Muuzika.Server.Models.Extensions.Room;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Models;

public class Room
{
    public readonly IJwtService JwtService;
    public string Code { get; set; }
    public Player Leader { get; set; }
    public Dictionary<string, Player> PlayersDictionary { get; set; } = new();
    public ImmutableArray<Player> Players => PlayersDictionary.Values.ToImmutableArray();
    public Round[]? Rounds { get; set; }
    
    public RoomStatus Status { get; set; } = RoomStatus.InLobby;
    public RoomPossibleRoundTypes PossibleRoundTypes { get; set; } = RoomPossibleRoundTypes.Both;
    
    public Timer? DeleteIfEmptyTimer { get; set; }

    public Room(IServiceProvider serviceProvider, string code, string leaderUsername)
    {
        JwtService = serviceProvider.GetRequiredService<IJwtService>();
        Code = code;
        Leader = this.AddPlayer(leaderUsername);
    }
}