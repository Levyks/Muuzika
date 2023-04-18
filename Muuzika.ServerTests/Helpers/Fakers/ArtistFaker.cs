using System.Collections.Immutable;
using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Models;

namespace Muuzika.ServerTests.Helpers.Fakers;

public static class ArtistFaker
{
    public static ImmutableArray<Artist> GetArtists(int count)
    {
        return Enumerable.Range(0, count)
            .Select(i => new Artist(SongProvider.Spotify, $"id-{i}", $"name-{i}"))
            .ToImmutableArray();
    }
    
}