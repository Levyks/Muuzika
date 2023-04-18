using System.Collections.Immutable;
using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Models;

public class Song : BaseProviderObject
{
    public string PreviewUrl { get; }
    public IEnumerable<Artist> Artists { get; }
    
    public Song(SongProvider provider, string id, string name, string previewUrl, IEnumerable<Artist> artists) : base(provider, id, name)
    {
        PreviewUrl = previewUrl;
        Artists = artists.ToImmutableArray();
    }
}