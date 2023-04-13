namespace Muuzika.Server.Dtos.Spotify;

public record SpotifyPlaylistOwnerDto(
    string DisplayName
)
{
    public const string Fields = "display_name";
}