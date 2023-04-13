namespace Muuzika.Server.Dtos.Spotify;

public record SpotifyPaginationDto<T>(
    IEnumerable<T> Items
)
{
    public static string Fields(string itemsFields) => $"items({itemsFields})";
}
    
