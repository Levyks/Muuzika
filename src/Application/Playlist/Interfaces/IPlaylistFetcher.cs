using Models = Muuzika.Domain.Models;

namespace Muuzika.Application.Playlist.Interfaces;

public interface IPlaylistFetcher
{
    Task<Models.Playlist.Playlist> FetchPlaylistAsync(string url);
}