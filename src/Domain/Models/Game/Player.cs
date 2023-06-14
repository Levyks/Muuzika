namespace Muuzika.Domain.Models;

public class Player: IEquatable<Player>
{
    public string Username { get; }
    public bool IsOnline { get; set; }
    public bool IsLeader { get; set; }
    
    public Player(string username)
    {
        Username = username;
    }

    public bool Equals(Player? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Username == other.Username;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Player)obj);
    }

    public override int GetHashCode()
    {
        return Username.GetHashCode();
    }
}