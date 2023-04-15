using System.Collections.Immutable;
using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Models.Interfaces;

public interface IPlaylist
{
    SongProvider Provider { get; }
    string Id { get; }
    string Name { get; }
    string CreatedBy { get; }
    string Url { get; }
    string ImageUrl { get; }
    
    int NumberOfPlayableSongRounds { get; }
    int NumberOfPlayableArtistRounds { get; }

    IEnumerable<Song> Songs { get; }
    IEnumerable<Song> SongsNotPlayed { get; }
    IEnumerable<Artist> ArtistsWithSongsNotPlayed { get; }
    IEnumerable<Song> GetSongsNotPlayedFromArtist(Artist artist);
    
    void MarkSongAsPlayed(Song song);
}