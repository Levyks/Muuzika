using System.Collections.Immutable;
using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Models;

public class Song : BaseProviderObject
{
    public string PreviewUrl { get; }
    public string ImageUrl { get; }
    public IEnumerable<Artist> Artists { get; }
    
    public Song(SongProvider provider, string id, string name, string url, string previewUrl, string imageUrl, IEnumerable<Artist> artists) : base(provider, id, name, url)
    {
        PreviewUrl = previewUrl;
        ImageUrl = imageUrl;
        Artists = artists.ToImmutableArray();
    }
}