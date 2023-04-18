namespace Muuzika.Server.Dtos.Spotify;

public record SpotifyArtistDto(
    string Id,
    string Name
)
{
    public const string Fields = "id,name";
}