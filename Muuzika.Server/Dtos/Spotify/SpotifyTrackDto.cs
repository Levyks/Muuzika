namespace Muuzika.Server.Dtos.Spotify;

public record SpotifyTrackDto(
    string Id,
    string Name,
    string? PreviewUrl,
    SpotifyExternalUrlsDto ExternalUrls,
    IEnumerable<SpotifyArtistDto> Artists,
    SpotifyAlbumDto Album
)
{
    public const string Fields = $"id,name,preview_url,external_urls({SpotifyExternalUrlsDto.Fields}),artists({SpotifyArtistDto.Fields}),album({SpotifyAlbumDto.Fields})";
}