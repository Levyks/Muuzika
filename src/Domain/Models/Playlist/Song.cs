namespace Muuzika.Domain.Models.Playlist;

public record Song(
    string Name,
    string Url,
    string ThumbnailUrl,
    string PlaybackUrl,
    ICollection<Artist> Artists
);