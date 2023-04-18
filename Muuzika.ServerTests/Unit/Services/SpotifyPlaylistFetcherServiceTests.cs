using System.Web;
using Moq;
using Muuzika.Server.Dtos.Spotify;
using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Enums.Spotify;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Models;
using Muuzika.Server.Providers.Interfaces;
using Muuzika.Server.Services;
using Muuzika.Server.Services.Interfaces;
using Muuzika.Server.Services.Playlist;

namespace Muuzika.ServerTests.Unit.Services;

public class SpotifyPlaylistFetcherServiceTests
{
    private DateTime _now;
    private Mock<IHttpService> _accountsApiHttpServiceMock = null!;
    private Mock<IHttpService> _webApiHttpServiceMock = null!;
    private Mock<ISpotifyMapper> _spotifyMapperMock = null!;
    private Mock<IDateTimeProvider> _dateTimeProviderMock = null!;
    
    private const string ClientId = "clientId";
    private const string ClientSecret = "clientSecret";
    private const int FetchLimit = 10;
    
    [SetUp]
    public void Setup()
    {
        _accountsApiHttpServiceMock = new Mock<IHttpService>();
        _webApiHttpServiceMock = new Mock<IHttpService>();
        _spotifyMapperMock = new Mock<ISpotifyMapper>();

        _now = new DateTime(2021, 1, 5);
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(x => x.GetNow()).Returns(() => _now);
    }

    private static SpotifyPlaylistItemDto CreatePlaylistItem(int idx)
    {
        var track = new SpotifyTrackDto(
            Id: $"track-{idx}-id",
            Name: $"track-{idx}-name",
            PreviewUrl: $"track-{idx}-previewUrl",
            Artists: new[]
            {
                new SpotifyArtistDto(
                    Id: $"track-{idx}-artist-id",
                    Name: $"track-{idx}-artist-name"
                )
            }
        );
        return new SpotifyPlaylistItemDto(track);
    }

    [Test]
    public void Constructor_SetsCorrectBaseAddresses()
    {
        _accountsApiHttpServiceMock.SetupSet(x => x.BaseAddress = It.IsAny<Uri>());
        _webApiHttpServiceMock.SetupSet(x => x.BaseAddress = It.IsAny<Uri>());
        
        _ = new SpotifyPlaylistFetcherService(ClientId, ClientSecret, FetchLimit, _spotifyMapperMock.Object, _dateTimeProviderMock.Object, _accountsApiHttpServiceMock.Object, _webApiHttpServiceMock.Object);
        
        _accountsApiHttpServiceMock.VerifySet(x => x.BaseAddress = new Uri("https://accounts.spotify.com"));
        _webApiHttpServiceMock.VerifySet(x => x.BaseAddress = new Uri("https://api.spotify.com"));
    }

    [Test]
    public async Task FetchPlaylistAsync_HappyPath()
    {
        const string accessToken = "accessToken";
        const int expiresIn = 3600;
        
        const string playlistId = "playlistId";
        
        const int totalTracks = FetchLimit * 5 / 2;
        
        const string expectedUrlInfo = 
            $"/v1/playlists/{playlistId}?fields=id%2cname%2cowner(display_name)%2cexternal_urls(spotify)%2cimages%2ctracks(total%2citems(track(id%2cname%2cpreview_url%2cartists(id%2cname))))";

        var expectedHeaders = new Dictionary<string, string> { ["Authorization"] = $"Bearer {accessToken}" };
        
        var expectedHttpContentAccessToken = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = ClientId,
            ["client_secret"] = ClientSecret
        });
        
        var allTracks = new List<SpotifyTrackDto>();
        
        var firstTrackPage = Enumerable.Range(0, FetchLimit).Select(CreatePlaylistItem).ToList();
        allTracks.AddRange(firstTrackPage.Select(x => x.Track));
        
        var infoWithTracks = new SpotifyPlaylistInfoWithTracksFirstPageDto(
            Id: playlistId,
            Name: "name",
            Owner: new SpotifyPlaylistOwnerDto("ownerDisplayName"),
            ExternalUrls: new SpotifyExternalUrlsDto("spotifyUrl"),
            Images: new[] { new SpotifyImageDto("imageUrl", 100, 100) },
            Tracks: new SpotifyPlaylistTracksFirstPageWithTotalDto(
                Total: totalTracks,
                Items: firstTrackPage
            )
        );

        string? passedUrlAccessToken = null;
        HttpContent? passedHttpContentAccessToken = null;
        Dictionary<string, string>? passedHeadersAccessToken = null;
        _accountsApiHttpServiceMock
            .Setup(x => x.PostAsync<SpotifyAccessTokenDto>(It.IsAny<string>(), It.IsAny<HttpContent>(), null))
            .Callback<string, HttpContent, Dictionary<string, string>>((url, httpContent, headers) =>
            {
                passedUrlAccessToken = url;
                passedHttpContentAccessToken = httpContent;
                passedHeadersAccessToken = headers;
            })
            .ReturnsAsync(new SpotifyAccessTokenDto(
                AccessToken: accessToken,
                TokenType: SpotifyTokenType.Bearer,
                ExpiresIn: expiresIn
            ));
        
        string? passedUrlInfo = null;
        Dictionary<string, string>? passedHeadersInfo = null;
        _webApiHttpServiceMock
            .Setup(x => x.GetAsync<SpotifyPlaylistInfoWithTracksFirstPageDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .Callback<string, Dictionary<string, string>>((url, headers) =>
            {
                passedUrlInfo = url;
                passedHeadersInfo = headers;
            })
            .ReturnsAsync(infoWithTracks);
        
        var passedUrlsTracks = new List<string>();
        var passedHeadersTracks = new List<Dictionary<string, string>>();
        _webApiHttpServiceMock
            .Setup(x => x.GetAsync<SpotifyPaginationDto<SpotifyPlaylistItemDto>>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .Callback<string, Dictionary<string, string>>((url, headers) =>
            {
                passedUrlsTracks.Add(url);
                passedHeadersTracks.Add(headers);
            })
            .Returns<string, Dictionary<string, string>>((_, _) =>
            {
                var offset = passedUrlsTracks.Count * FetchLimit;
                var totalOfPage = Math.Min(FetchLimit, totalTracks - offset);
                var items = Enumerable.Range(offset, totalOfPage).Select(CreatePlaylistItem).ToList();
                allTracks.AddRange(items.Select(x => x.Track));
                var pagination = new SpotifyPaginationDto<SpotifyPlaylistItemDto>(items);
                return Task.FromResult(pagination);
            });

        var expectedPlaylist = new Playlist(
            provider: SongProvider.Spotify,
            id: playlistId,
            name: "name",
            createdBy: "ownerDisplayName",
            url: "spotifyUrl",
            imageUrl: "imageUrl",
            songs: Enumerable.Empty<Song>()
        );
        
        SpotifyPlaylistInfoDto? passedInfo = null;
        List<SpotifyTrackDto>? passedTracks = null;
        _spotifyMapperMock
            .Setup(x => x.ToPlaylist(It.IsAny<SpotifyPlaylistInfoDto>(), It.IsAny<IEnumerable<SpotifyTrackDto>>()))
            .Callback<SpotifyPlaylistInfoDto, IEnumerable<SpotifyTrackDto>>((info, tracks) =>
            {
                passedInfo = info;
                passedTracks = tracks.ToList();
            })
            .Returns(expectedPlaylist);
            
        var spotifyPlaylistFetcherService = new SpotifyPlaylistFetcherService(ClientId, ClientSecret, FetchLimit, _spotifyMapperMock.Object, _dateTimeProviderMock.Object, _accountsApiHttpServiceMock.Object, _webApiHttpServiceMock.Object);
        var playlist = await spotifyPlaylistFetcherService.FetchPlaylistAsync(playlistId);
        
        Assert.Multiple(async () =>
        {
            Assert.That(playlist, Is.EqualTo(expectedPlaylist));

            Assert.That(passedInfo, Is.EqualTo(infoWithTracks));
            Assert.That(passedTracks, Is.EquivalentTo(allTracks));
            
            Assert.That(passedHeadersTracks, Has.Count.EqualTo(totalTracks / FetchLimit));
            Assert.That(passedHeadersTracks, Is.All.EqualTo(expectedHeaders));
            
            Assert.That(passedUrlsTracks, Has.Count.EqualTo(totalTracks / FetchLimit));
            AssertPassedUrlsTracks(playlistId, passedUrlsTracks);

            Assert.That(passedUrlInfo, Is.EqualTo(expectedUrlInfo));
            Assert.That(passedHeadersInfo, Is.EqualTo(expectedHeaders));
            
            Assert.That(passedUrlAccessToken, Is.EqualTo("/api/token"));
            await AssertPassedFormUrlEncodedContent(passedHttpContentAccessToken!, expectedHttpContentAccessToken);
            Assert.That(passedHeadersAccessToken, Is.Null);
        });
    }

    private static async Task AssertPassedFormUrlEncodedContent(HttpContent passedHttpContent, FormUrlEncodedContent expectedHttpContent)
    {
        Assert.That(passedHttpContent, Is.InstanceOf<FormUrlEncodedContent>());
        var passedFormUrlEncodedContent = (FormUrlEncodedContent) passedHttpContent;
        var formUrlEncodedContent = await passedFormUrlEncodedContent.ReadAsStringAsync();
        var expectedFormUrlEncodedContent = await expectedHttpContent.ReadAsStringAsync();
        Assert.That(formUrlEncodedContent, Is.EqualTo(expectedFormUrlEncodedContent));
    }
    
    
    private static void AssertPassedUrlsTracks(string playlistId, IReadOnlyCollection<string> passedUrlsTracks)
    {
        for (var i = 0; i < passedUrlsTracks.Count; i++)
        {
            var url = passedUrlsTracks.ElementAt(i);
            var expectedUrl =
                $"/v1/playlists/{playlistId}/tracks?fields=items(track(id%2cname%2cpreview_url%2cartists(id%2cname)))&offset={(i + 1) * FetchLimit}&limit={FetchLimit}";
            Assert.That(url, Is.EqualTo(expectedUrl));
        }
    }
}