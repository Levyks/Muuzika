namespace Muuzika.Server.Dtos.Spotify;


public record SpotifyPlaylistTracksFirstPageWithTotalDto(
    int Total,
    IEnumerable<SpotifyPlaylistItemDto> Items
)
{
    public const string Fields = $"total,items({SpotifyPlaylistItemDto.Fields})";
}