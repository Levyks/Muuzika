using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Dtos.Hub.Responses;

public record PlaylistDto(
    SongProvider Provider,
    string Id,
    string Name,
    string CreatedBy,
    string Url,
    string ImageUrl,
    int NumberOfSongs,
    int NumberOfNotPlayedSongs,
    int NumberOfPlayableSongRounds,
    int NumberOfPlayableArtistRounds
);