using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Muuzika.Server.Dtos.Spotify;
using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Models.Interfaces;
using Muuzika.Server.Providers.Interfaces;
using Muuzika.Server.Services.Interfaces;
using Muuzika.Server.Services.Playlist.Interfaces;

namespace Muuzika.Server.Services.Playlist;

public class SpotifyPlaylistFetcherService: IPlaylistFetcherService
{
    private readonly ISpotifyMapper _spotifyMapper;
    private readonly IDateTimeProvider _dateTimeProvider;
    
    private readonly IHttpService _accountsApiHttpService;
    private readonly IHttpService _webApiHttpService;

    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly int _fetchLimit;
    
    private string? _accessToken;
    private DateTime? _accessTokenExpirationTime;
    private Task<string>? _accessTokenTask;
    
    private static readonly TimeSpan AccessTokenExpirationTimeMargin = TimeSpan.FromSeconds(30);
    
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    
    public SongProvider Provides => SongProvider.Spotify;
    
    public SpotifyPlaylistFetcherService(string clientId, string clientSecret, int fetchLimit, ISpotifyMapper spotifyMapper, IDateTimeProvider dateTimeProvider, IHttpService accountsApiHttpService, IHttpService webApiHttpService)
    {
        _spotifyMapper = spotifyMapper;
        _dateTimeProvider = dateTimeProvider;

        _clientId = clientId;
        _clientSecret = clientSecret;
        _fetchLimit = fetchLimit;
        
        _accountsApiHttpService = accountsApiHttpService;
        _accountsApiHttpService.BaseAddress = new Uri("https://accounts.spotify.com");
        _accountsApiHttpService.JsonSerializerOptions = JsonSerializerOptions;
        
        _webApiHttpService = webApiHttpService;
        _webApiHttpService.BaseAddress = new Uri("https://api.spotify.com");
        _webApiHttpService.JsonSerializerOptions = JsonSerializerOptions;
    }
    
    public SpotifyPlaylistFetcherService(IConfigProvider configProvider, ISpotifyMapper spotifyMapper, IDateTimeProvider dateTimeProvider, IHttpService accountsApiHttpService, IHttpService webApiHttpService):
        this(configProvider.SpotifyClientId, configProvider.SpotifyClientSecret, 100, spotifyMapper, dateTimeProvider, accountsApiHttpService, webApiHttpService)
    {
    }

    private async Task<string> GenerateAccessTokenAsync()
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _clientId,
            ["client_secret"] = _clientSecret
        });
        var response = await _accountsApiHttpService.PostAsync<SpotifyAccessTokenDto>("/api/token", content);
        _accessTokenExpirationTime = _dateTimeProvider.GetNow().AddSeconds(response.ExpiresIn);
        return _accessToken = response.AccessToken;
    }
    
    private async ValueTask<string> GetAccessTokenAsync()
    {
        if (_accessToken != null && _accessTokenExpirationTime != null && 
            _accessTokenExpirationTime - _dateTimeProvider.GetNow() > AccessTokenExpirationTimeMargin)
            return _accessToken;
        
        if (_accessTokenTask != null)
            return await _accessTokenTask;
        
        _accessTokenTask = GenerateAccessTokenAsync();
        var accessToken = await _accessTokenTask;
        _accessTokenTask = null;
        
        return accessToken;
    }
    
    private async ValueTask<Dictionary<string, string>> GetHeadersAsync()
    {
        var accessToken = await GetAccessTokenAsync();
        return new Dictionary<string, string> { ["Authorization"] = $"Bearer {accessToken}" };
    }

    private async Task<SpotifyPlaylistInfoWithTracksFirstPageDto> FetchPlaylistInfoWithTracksFirstPage(
        string playlistId)
    {
        var headers = await GetHeadersAsync();
        var fieldsEncoded = HttpUtility.UrlEncode(SpotifyPlaylistInfoWithTracksFirstPageDto.Fields);
        var url = $"/v1/playlists/{playlistId}?fields={fieldsEncoded}";
        
        return await _webApiHttpService.GetAsync<SpotifyPlaylistInfoWithTracksFirstPageDto>(url, headers);
    }
    
    private async Task<SpotifyPaginationDto<SpotifyPlaylistItemDto>> FetchPlaylistTracksPage(
        string playlistId, int offset, int limit)
    {
        var headers = await GetHeadersAsync();
        var fieldsEncoded = HttpUtility.UrlEncode(SpotifyPaginationDto<SpotifyPlaylistItemDto>.Fields(SpotifyPlaylistItemDto.Fields));
        var url = $"/v1/playlists/{playlistId}/tracks?fields={fieldsEncoded}&offset={offset}&limit={limit}";
        
        return await _webApiHttpService.GetAsync<SpotifyPaginationDto<SpotifyPlaylistItemDto>>(url, headers);
    }
    
    private async Task<IEnumerable<SpotifyTrackDto>> FetchRemainingPlaylistTracks(
        string playlistId, int total, int offset, int limit)
    {
        var numberOfRemainingPages = (int) Math.Ceiling((double) (total - offset) / limit);
        
        var offsets = Enumerable.Range(0, numberOfRemainingPages)
            .Select(pageNumber => offset + pageNumber * limit);
        
        var tasks = offsets.Select(o => FetchPlaylistTracksPage(playlistId, o, limit));
        
        var pages = await Task.WhenAll(tasks);
        
        return pages.SelectMany(p => p.Items.Select(i => i.Track));
    }

    public async Task<IPlaylist> FetchPlaylistAsync(string playlistId)
    {
        var info = await FetchPlaylistInfoWithTracksFirstPage(playlistId);
        
        var tracks = info.Tracks.Items.Select(i => i.Track).ToList();

        if (info.Tracks.Total > info.Tracks.Items.Count())
        {
            var remainingTracks = await FetchRemainingPlaylistTracks(
                playlistId, info.Tracks.Total, info.Tracks.Items.Count(), _fetchLimit);
            tracks.AddRange(remainingTracks);
        }
        
        return _spotifyMapper.ToPlaylist(info, tracks);
    }
}