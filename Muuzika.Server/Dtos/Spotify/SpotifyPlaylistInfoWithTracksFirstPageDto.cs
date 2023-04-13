namespace Muuzika.Server.Dtos.Spotify;


public record SpotifyPlaylistInfoWithTracksFirstPageDto(
    string Id,
    string Name,
    SpotifyPlaylistOwnerDto Owner,
    SpotifyExternalUrlsDto ExternalUrls,
    IEnumerable<SpotifyImageDto> Images,
    SpotifyPlaylistTracksFirstPageWithTotalDto Tracks
): SpotifyPlaylistInfoDto(Id, Name, Owner, ExternalUrls, Images)
{
    public new const string Fields = $"{SpotifyPlaylistInfoDto.Fields},tracks({SpotifyPlaylistTracksFirstPageWithTotalDto.Fields})";
}