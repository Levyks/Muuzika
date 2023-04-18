using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Models;

public class Artist: BaseProviderObject
{
    public Artist(SongProvider provider, string id, string name) : base(provider, id, name)
    {
    }
}