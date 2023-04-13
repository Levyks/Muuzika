using Muuzika.Server.Dtos.Gateway;
using Muuzika.Server.Dtos.Hub;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Models;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Mappers;

public class RoomMapper: IRoomMapper
{
    private readonly IPlayerMapper _playerMapper;
    
    public RoomMapper(IPlayerMapper playerMapper)
    {
        _playerMapper = playerMapper;
    }

    public RoomDto ToDto(Room room)
    {
        return new RoomDto(
            Code: room.Code,
            LeaderUsername: room.Leader.Username,
            Status: room.Status,
            Players: room.ServiceProvider.GetRequiredService<IRoomPlayerService>().GetPlayers().Select(_playerMapper.ToDto),
            Options: room.Options
        );
    }

    public RoomCreatedOrJoinedDto ToCreatedOrJoinedDto(Room room, Player player)
    {
        return new RoomCreatedOrJoinedDto(
            Username: player.Username,
            RoomCode: room.Code,
            Token: room.ServiceProvider.GetRequiredService<IRoomPlayerService>().GetTokenForPlayer(player)
        );
    }
    
    public StateSyncDto ToStateSyncDto(Room room, Player player)
    {
        return new StateSyncDto(
            Room: ToDto(room),
            Player: _playerMapper.ToDto(player)
        );
    }
}