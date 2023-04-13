using System.Collections.Immutable;
using Muuzika.Server.Extensions;
using Muuzika.Server.Models;
using Muuzika.Server.Models.Interfaces;
using Muuzika.Server.Providers.Interfaces;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Services;

public class PlaylistPickerService: IPlaylistPickerService
{
    private readonly IRandomProvider _randomProvider;
    
    public PlaylistPickerService(IRandomProvider randomProvider)
    {
        _randomProvider = randomProvider;
    }

    public ImmutableArray<Song> PickOptions(IPlaylist playlist, int maxNumberOfOptions)
    {
        if (playlist.SongsNotPlayed.Count() < 2)
        {
            throw new Exception("Not enough not-played songs");
        }
        
        var numberOfOptions = Math.Min(maxNumberOfOptions, playlist.SongsNotPlayed.Count());
        var random = _randomProvider.GetRandom();
        
       return playlist.SongsNotPlayed
            .TakeRandom(numberOfOptions, random)
            .ToImmutableArray();
    }

    public ImmutableArray<Song> PickOptionsAvoidingRepeatedArtists(IPlaylist playlist, int maxNumberOfOptions)
    {
        if (playlist.ArtistsWithSongsNotPlayed.Count() < 2)
        {
            throw new Exception("Not enough artists with not-played songs");
        }
        
        var numberOfOptions = Math.Min(maxNumberOfOptions, playlist.ArtistsWithSongsNotPlayed.Count());
        var random = _randomProvider.GetRandom();

        return playlist.ArtistsWithSongsNotPlayed
            .TakeRandom(numberOfOptions, random)
            .Select(artist => playlist.GetSongsNotPlayedFromArtist(artist).PickRandom(random))
            .ToImmutableArray();
    }
}