using Muuzika.Server.Models;

namespace Muuzika.Server.Services.Interfaces;

public interface IPlaylistFetcherService
{
    Task<Playlist> FetchPlaylistAsync(string playlistId);
}