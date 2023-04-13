using System.Collections.Immutable;
using Muuzika.Server.Dtos.Spotify;
using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Models;

namespace Muuzika.Server.Mappers;

public class SpotifyMapper: ISpotifyMapper
{
    public Playlist ToPlaylist(SpotifyPlaylistInfoDto playlistInfoDto, IEnumerable<SpotifyTrackDto> tracks)
    {
        return new Playlist(
            SongProvider.Spotify,
            playlistInfoDto.Id,
            playlistInfoDto.Name,
            playlistInfoDto.ExternalUrls.Spotify,
            playlistInfoDto.Images.First().Url,
            tracks.Where(t => t.PreviewUrl != null).Select(ToSong)
        );
    }
    
    public Song ToSong(SpotifyTrackDto track)
    {
        if (track.PreviewUrl == null)
            throw new ArgumentException("Track must have a preview url", nameof(track));
        
        return new Song(
            SongProvider.Spotify,
            track.Id,
            track.Name,
            track.ExternalUrls.Spotify,
            track.PreviewUrl,
            track.Album.Images.First().Url,
            track.Artists.Select(ToArtist)
        );
    }
    
    public Artist ToArtist(SpotifyArtistDto artist)
    {
        return new Artist(
            SongProvider.Spotify,
            artist.Id,
            artist.Name,
            artist.ExternalUrls.Spotify
        );
    }
}