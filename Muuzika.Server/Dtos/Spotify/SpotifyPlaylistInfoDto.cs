namespace Muuzika.Server.Dtos.Spotify;

public record SpotifyPlaylistInfoDto(    
    string Id,
    string Name,
    SpotifyPlaylistOwnerDto Owner,
    SpotifyExternalUrlsDto ExternalUrls,
    IEnumerable<SpotifyImageDto> Images
)
{
    public const string Fields = $"id,name,owner({SpotifyPlaylistOwnerDto.Fields}),external_urls({SpotifyExternalUrlsDto.Fields}),images";
}