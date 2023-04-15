using Muuzika.Server.Dtos.Hub;
using Muuzika.Server.Dtos.Hub.Responses;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Models.Interfaces;

namespace Muuzika.Server.Mappers;

public class PlaylistMapper: IPlaylistMapper
{
    public PlaylistDto ToDto(IPlaylist playlist)
    {
        return new PlaylistDto(
            Provider: playlist.Provider,
            Id: playlist.Id,
            Name: playlist.Name,
            CreatedBy: playlist.CreatedBy,
            Url: playlist.Url,
            ImageUrl: playlist.ImageUrl,
            NumberOfSongs: playlist.Songs.Count(),
            NumberOfNotPlayedSongs: playlist.SongsNotPlayed.Count(),
            NumberOfPlayableSongRounds: playlist.NumberOfPlayableSongRounds,
            NumberOfPlayableArtistRounds: playlist.NumberOfPlayableArtistRounds
        );
    }
}