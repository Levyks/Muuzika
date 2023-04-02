using Muuzika.Server.Dtos.Hub;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Models;

namespace Muuzika.Server.Mappers;

public class PlayerMapper: IPlayerMapper
{
    public PlayerDto ToDto(Player player)
    {
        return new PlayerDto(
            Username: player.Username, 
            IsConnected: player.IsConnected, 
            Score: player.Score
        );
    }
}