using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Models.Interfaces;

namespace Muuzika.Server.Services.Playlist.Interfaces;

public interface IPlaylistFetcherService
{
    SongProvider Provides { get; }
    Task<IPlaylist> FetchPlaylistAsync(string playlistId);
}