using System.Collections.Immutable;
using Muuzika.Server.Dtos.Spotify;
using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Models;
using Muuzika.Server.Models.Interfaces;

namespace Muuzika.Server.Mappers;

public class SpotifyMapper: ISpotifyMapper
{
    public IPlaylist ToPlaylist(SpotifyPlaylistInfoDto playlistInfoDto, IEnumerable<SpotifyTrackDto> tracks)
    {
        return new Playlist(
            provider: SongProvider.Spotify,
            id: playlistInfoDto.Id,
            name: playlistInfoDto.Name,
            createdBy: playlistInfoDto.Owner.DisplayName,
            url: playlistInfoDto.ExternalUrls.Spotify,
            imageUrl: playlistInfoDto.Images.First().Url,
            songs: tracks.Where(t => t.PreviewUrl != null).Select(ToSong)
        );
    }
    
    public Song ToSong(SpotifyTrackDto track)
    {
        if (track.PreviewUrl == null)
            throw new ArgumentException("Track must have a preview url", nameof(track));
        
        return new Song(
            provider: SongProvider.Spotify,
            id: track.Id,
            name: track.Name,
            previewUrl: track.PreviewUrl,
            artists: track.Artists.Select(ToArtist)
        );
    }
    
    public Artist ToArtist(SpotifyArtistDto artist)
    {
        return new Artist(
            provider: SongProvider.Spotify,
            id: artist.Id,
            name: artist.Name
        );
    }
}