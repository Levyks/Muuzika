namespace Muuzika.Domain.Models.Playlist;

public record Playlist(
    string Name,
    string Url,
    string ThumbnailUrl,
    string Creator,
    ICollection<Song> Songs
);