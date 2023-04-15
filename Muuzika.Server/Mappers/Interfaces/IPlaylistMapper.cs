using Muuzika.Server.Dtos.Hub;
using Muuzika.Server.Dtos.Hub.Responses;
using Muuzika.Server.Models.Interfaces;

namespace Muuzika.Server.Mappers.Interfaces;

public interface IPlaylistMapper
{
    PlaylistDto ToDto(IPlaylist playlist);
}