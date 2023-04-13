using Muuzika.Server.Dtos.Spotify;
using Muuzika.Server.Models;

namespace Muuzika.Server.Mappers.Interfaces;

public interface ISpotifyMapper
{
    Playlist ToPlaylist(SpotifyPlaylistInfoDto playlistInfoDto, IEnumerable<SpotifyTrackDto> tracks);
    Song ToSong(SpotifyTrackDto track);
    Artist ToArtist(SpotifyArtistDto artist);
}