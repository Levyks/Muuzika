using System.Collections.Immutable;
using Muuzika.Server.Extensions;
using Muuzika.Server.Models;
using Muuzika.Server.Models.Interfaces;
using Muuzika.Server.Providers.Interfaces;
using Muuzika.Server.Services.Playlist.Interfaces;

namespace Muuzika.Server.Services.Playlist;

public class PlaylistSongPickerService: IPlaylistSongPickerService
{
    private readonly IRandomProvider _randomProvider;
    
    public PlaylistSongPickerService(IRandomProvider randomProvider)
    {
        _randomProvider = randomProvider;
    }

    public ImmutableArray<Song> PickSongs(IPlaylist playlist, int maxNumberOfSongs)
    {
        if (playlist.SongsNotPlayed.Count() < 2)
        {
            throw new Exception("Not enough not-played songs");
        }
        
        var numberOfOptions = Math.Min(maxNumberOfSongs, playlist.SongsNotPlayed.Count());
        var random = _randomProvider.GetRandom();
        
       return playlist.SongsNotPlayed
            .TakeRandom(numberOfOptions, random)
            .ToImmutableArray();
    }

    public ImmutableArray<Song> PickSongsFromDifferentArtists(IPlaylist playlist, int maxNumberOfSongs)
    {
        if (playlist.ArtistsWithSongsNotPlayed.Count() < 2)
        {
            throw new Exception("Not enough artists with not-played songs");
        }
        
        var numberOfOptions = Math.Min(maxNumberOfSongs, playlist.ArtistsWithSongsNotPlayed.Count());
        var random = _randomProvider.GetRandom();

        return playlist.ArtistsWithSongsNotPlayed
            .TakeRandom(numberOfOptions, random)
            .Select(artist => playlist.GetSongsNotPlayedFromArtist(artist).PickRandom(random))
            .ToImmutableArray();
    }
}