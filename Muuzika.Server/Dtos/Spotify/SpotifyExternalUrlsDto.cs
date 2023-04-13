namespace Muuzika.Server.Dtos.Spotify;

public record SpotifyExternalUrlsDto(
    string Spotify
)
{
    public const string Fields = "spotify";
}