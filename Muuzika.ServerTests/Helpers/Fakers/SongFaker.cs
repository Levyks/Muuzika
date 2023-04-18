using System.Collections.Immutable;
using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Models;

namespace Muuzika.ServerTests.Helpers.Fakers;

public static class SongFaker
{
    public static ImmutableArray<Song> GetSongs(int numberOfDifferentArtists = 5, int[]? numberOfSongsPerArtist = null)
    {
        numberOfSongsPerArtist ??= Enumerable.Range(0, numberOfDifferentArtists)
            .Select(i => i + 1)
            .ToArray();
        
        var artists = ArtistFaker.GetArtists(numberOfDifferentArtists);

        var songs = artists
            .Zip(numberOfSongsPerArtist, GetSongs)
            .SelectMany(x => x)
            .ToImmutableArray();
        
        return songs;
    }
    
    public static ImmutableArray<Song> GetSongs(Artist artist, int numberOfSongs = 5)
    {
        return Enumerable.Range(0, numberOfSongs)
            .Select(i => new Song(SongProvider.Spotify, $"{artist.Id}-{i}" , $"{artist.Name}-{i}", "previewUrl", ImmutableArray.Create(artist)))
            .ToImmutableArray();
    }
}