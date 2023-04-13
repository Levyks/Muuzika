namespace Muuzika.Server.Dtos.Spotify;

public record SpotifyPlaylistItemDto(
    SpotifyTrackDto Track
)
{
    public const string Fields = $"track({SpotifyTrackDto.Fields})";
}