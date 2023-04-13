using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Models;

public abstract class BaseProviderObject: IEquatable<BaseProviderObject>
{
    public SongProvider Provider { get; } 
    public string Id { get; }
    public string Name { get; }
    public string Url { get; }

    protected BaseProviderObject(SongProvider provider, string id, string name, string url)
    {
        Provider = provider;
        Id = id;
        Name = name;
        Url = url;
    }

    public bool Equals(BaseProviderObject? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Provider == other.Provider && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((BaseProviderObject)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Provider, Id);
    }
}