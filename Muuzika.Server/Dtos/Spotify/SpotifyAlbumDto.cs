namespace Muuzika.Server.Dtos.Spotify;

public record SpotifyAlbumDto(
    IEnumerable<SpotifyImageDto> Images
)
{
    public const string Fields = "images";
}