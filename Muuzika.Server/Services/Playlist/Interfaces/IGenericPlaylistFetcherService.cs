using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Models.Interfaces;

namespace Muuzika.Server.Services.Playlist.Interfaces;

public interface IGenericPlaylistFetcherService
{
    Task<IPlaylist> FetchPlaylistAsync(SongProvider provider, string playlistId);
}