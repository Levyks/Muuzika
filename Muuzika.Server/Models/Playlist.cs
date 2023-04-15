using System.Collections.Immutable;
using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Models.Interfaces;

namespace Muuzika.Server.Models;

public class Playlist: BaseProviderObject, IPlaylist
{
    public string CreatedBy { get; }
    public string ImageUrl { get; }
    
    public int NumberOfPlayableSongRounds => Math.Max(_songsNotPlayed.Count - 1, 0);
    public int NumberOfPlayableArtistRounds { get; }
    
    public IEnumerable<Song> Songs => _songs;
    public IEnumerable<Song> SongsNotPlayed => _songsNotPlayed;
    public IEnumerable<Artist> ArtistsWithSongsNotPlayed => _songsNotPlayedByArtist.Keys;
    
    private readonly ImmutableList<Song> _songs;
    private readonly HashSet<Song> _songsNotPlayed;
    private readonly Dictionary<Artist, HashSet<Song>> _songsNotPlayedByArtist;

    public Playlist(SongProvider provider, string id, string name, string createdBy, string url, string imageUrl, IEnumerable<Song> songs): base(provider, id, name, url)
    {
        _songs = songs.ToImmutableList();
        _songsNotPlayed = new HashSet<Song>(_songs);
        _songsNotPlayedByArtist = GetSongsNotPlayedByArtist(_songs);
        NumberOfPlayableArtistRounds = GetNumberOfPlayableArtistRounds();
        ImageUrl = imageUrl;
        CreatedBy = createdBy;
    }

    public IEnumerable<Song> GetSongsNotPlayedFromArtist(Artist artist)
    {
        return _songsNotPlayedByArtist[artist];
    }

    public void MarkSongAsPlayed(Song song)
    {
        var artist = song.Artists.First();
        _songsNotPlayed.Remove(song);
        _songsNotPlayedByArtist[artist].Remove(song);
        if (_songsNotPlayedByArtist[artist].Count == 0)
        {
            _songsNotPlayedByArtist.Remove(artist);
        }
    }
    
    private int GetNumberOfPlayableArtistRounds()
    {
        return _songsNotPlayedByArtist
            .Values.Select(x => x.Count)
            .Order()
            .SkipLast(1)
            .Sum();
    }
    
    private static Dictionary<Artist, HashSet<Song>> GetSongsNotPlayedByArtist(IEnumerable<Song> songs)
    {
        var songsNotPlayedByArtist = new Dictionary<Artist, HashSet<Song>>();
        
        foreach (var song in songs)
        {
            var artist = song.Artists.First();

            if (!songsNotPlayedByArtist.ContainsKey(artist))
            {
                songsNotPlayedByArtist.Add(artist, new HashSet<Song> { song });   
            }
            else
            {
                songsNotPlayedByArtist[artist].Add(song);
            }
        }

        return songsNotPlayedByArtist;
    }

}