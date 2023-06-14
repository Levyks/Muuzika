using Muuzika.Application.Helpers.Interfaces;
using Muuzika.Application.Playlist.Interfaces;
using Muuzika.Domain.Enums;
using Muuzika.Domain.Models.Playlist;
using Models = Muuzika.Domain.Models;

namespace Muuzika.Application.Playlist;

internal class PlaylistWrapper: IPlaylistWrapper
{
    private readonly IRandomHelper _randomHelper;
    public Models.Playlist.Playlist Playlist => _playlist ?? throw new NullReferenceException();
    public int NumberOfSongs => Playlist.Songs.Count;
    public int NumberOfPlayedSongs => NumberOfSongs - _songsNotPlayed.Count;
    
    private Models.Playlist.Playlist? _playlist; 
    private HashSet<Song> _songsNotPlayed = null!;
    private Dictionary<Artist, HashSet<Song>> _songsNotPlayedByArtist = null!;
    private int NumberOfPlayableSongRounds => Math.Max(_songsNotPlayed.Count - 1, 0);
    private int NumberOfPlayableArtistsRounds => _songsNotPlayedByArtist
        .Values.Select(x => x.Count)
        .Order()
        .SkipLast(1)
        .Sum();
    
    public PlaylistWrapper(IRandomHelper randomHelper)
    {
        _randomHelper = randomHelper;
    }

    public int GetNumberOfPlayableRounds(RoundType roundType) => roundType switch
    {
        RoundType.Song => NumberOfPlayableSongRounds,
        RoundType.Artists => NumberOfPlayableArtistsRounds,
        _ => throw new ArgumentOutOfRangeException(nameof(roundType), roundType, null)
    };

    public (Song, ICollection<Song>) GetSongAndChoices(RoundType roundType, int maxNumberOfChoices)
    {
        var numberOfPlayableRounds = GetNumberOfPlayableRounds(roundType);
        
        if (numberOfPlayableRounds == 0)
        {
            throw new InvalidOperationException("No more rounds to play");
        }
        
        var choices = roundType switch
        {
            RoundType.Song => GetSongRoundChoices(maxNumberOfChoices),
            RoundType.Artists => GetArtistsRoundChoices(maxNumberOfChoices),
            _ => throw new ArgumentOutOfRangeException(nameof(roundType), roundType, null)
        };
        
        var song = _randomHelper.TakeRandom(_songsNotPlayed);
        MarkSongAsPlayed(song);
        
        return (song, choices);
    }
    
    public void Load(Models.Playlist.Playlist playlist)
    {
        _playlist = playlist;
        _songsNotPlayed = new HashSet<Song>(Playlist.Songs);
        _songsNotPlayedByArtist = Playlist.Songs
            .GroupBy(x => x.Artists.First())
            .ToDictionary(x => x.Key, x => new HashSet<Song>(x));
    }

    public void Reset()
    {
        Load(Playlist);
    }
    
    private void MarkSongAsPlayed(Song song)
    {
        _songsNotPlayed.Remove(song);
        var artist = song.Artists.First();
        var songsNotPlayedOfArtist = _songsNotPlayedByArtist[artist];
        songsNotPlayedOfArtist.Remove(song);
        if (songsNotPlayedOfArtist.Count == 0)
        {
            _songsNotPlayedByArtist.Remove(artist);
        }
    }
    
    private ICollection<Song> GetSongRoundChoices(int maxNumberOfChoices)
    {
        var numberOfChoices = Math.Min(maxNumberOfChoices, _songsNotPlayed.Count);
        return _randomHelper.TakeRandom(_songsNotPlayed, numberOfChoices);
    }
    
    private ICollection<Song> GetArtistsRoundChoices(int maxNumberOfChoices)
    {
        var numberOfChoices = Math.Min(maxNumberOfChoices, _songsNotPlayedByArtist.Count);
        var artists = _randomHelper.TakeRandom(_songsNotPlayedByArtist.Keys, numberOfChoices);
        return artists.Select(artist => _randomHelper.TakeRandom(_songsNotPlayedByArtist[artist])).ToList();
    }
}