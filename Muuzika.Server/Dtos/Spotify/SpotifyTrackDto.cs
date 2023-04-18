namespace Muuzika.Server.Dtos.Spotify;

public record SpotifyTrackDto(
    string Id,
    string Name,
    string? PreviewUrl,
    IEnumerable<SpotifyArtistDto> Artists
)
{
    public const string Fields = $"id,name,preview_url,artists({SpotifyArtistDto.Fields})";
}