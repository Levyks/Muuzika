namespace Muuzika.Server.Dtos.Spotify;

public record SpotifyArtistDto(
    string Id,
    string Name, 
    SpotifyExternalUrlsDto ExternalUrls
)
{
    public const string Fields = $"id,external_urls({SpotifyExternalUrlsDto.Fields}),name";
}