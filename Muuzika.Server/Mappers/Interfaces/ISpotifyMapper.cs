using Muuzika.Server.Dtos.Spotify;
using Muuzika.Server.Models;
using Muuzika.Server.Models.Interfaces;

namespace Muuzika.Server.Mappers.Interfaces;

public interface ISpotifyMapper
{
    IPlaylist ToPlaylist(SpotifyPlaylistInfoDto playlistInfoDto, IEnumerable<SpotifyTrackDto> tracks);
    Song ToSong(SpotifyTrackDto track);
    Artist ToArtist(SpotifyArtistDto artist);
}