using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Models;

public class Artist: BaseProviderObject
{
    public Artist(SongProvider provider, string id, string name, string url) : base(provider, id, name, url)
    {
    }
}