using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Dtos.Hub.Requests;

public record SetPlaylistDto(
    SongProvider Provider,
    string Id
);