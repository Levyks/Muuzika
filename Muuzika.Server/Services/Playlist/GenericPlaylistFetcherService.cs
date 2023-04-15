using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Models.Interfaces;
using Muuzika.Server.Services.Playlist.Interfaces;

namespace Muuzika.Server.Services.Playlist;

public class GenericPlaylistFetcherService: IGenericPlaylistFetcherService
{
    private readonly Dictionary<SongProvider, IPlaylistFetcherService> _playlistFetcherServices;
    
    public GenericPlaylistFetcherService(IEnumerable<IPlaylistFetcherService> playlistFetcherServices)
    {
        _playlistFetcherServices = playlistFetcherServices.ToDictionary(x => x.Provides);
    }
    
    public async Task<IPlaylist> FetchPlaylistAsync(SongProvider provider, string playlistId)
    {
        if (!_playlistFetcherServices.TryGetValue(provider, out var playlistFetcherService))
            throw new ArgumentException($"No playlist fetcher service found for provider {provider}");
        
        return await playlistFetcherService.FetchPlaylistAsync(playlistId);
    }
}             