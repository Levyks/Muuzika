using Muuzika.Server.Enums.Spotify;

namespace Muuzika.Server.Dtos.Spotify;

public record SpotifyAccessTokenDto(
    string AccessToken,
    SpotifyTokenType TokenType,
    int ExpiresIn
    );