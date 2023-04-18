using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Muuzika.Server.Dtos.Spotify;
using Muuzika.Server.Enums.Spotify;
using Muuzika.Server.Services;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.ServerTests.E2E.Helpers.Mockers;

public class SpotifyMocker
{
    
    private readonly IConfiguration _configuration;
    private IHttpService? HttpService { get; set; }

    private SpotifyMocker(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private async Task<HttpResponseMessage> HandleTokenRequest(HttpRequestMessage request)
    {
        var bodyString = await request.Content?.ReadAsStringAsync()!;
        
        var body  = bodyString
            .Split('&')
            .Select(x => x.Split('='))
            .ToDictionary(x => x[0], x => x[1]);


        if (body["grant_type"] != "client_credentials" ||
            body["client_id"] != _configuration["Spotify:ClientId"] ||
            body["client_secret"] != _configuration["Spotify:ClientSecret"])
        {
            throw new Exception($"Unexpected request: {request}");
        }

        var responseContent = new SpotifyAccessTokenDto("foo", SpotifyTokenType.Bearer, 3600);

        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(responseContent,  options: HttpService?.JsonSerializerOptions)
        };
    }
    
    private async Task<HttpResponseMessage> HandlePlaylistRequest(HttpRequestMessage request)
    {
        if (request.RequestUri?.Query != SpotifyPlaylistInfoWithTracksFirstPageDto.Fields)
        {
            throw new Exception($"Unexpected request: {request}");
        }
        
        var playlistId = request.RequestUri.Segments[^1];

        throw new NotImplementedException();
    }
    

    private Task<HttpResponseMessage> HandleSendAsync(HttpRequestMessage request, CancellationToken _) => request.RequestUri?.ToString() switch
    {
        "https://accounts.spotify.com/api/token" => HandleTokenRequest(request),
        { } url when url.StartsWith("https://api.spotify.com/v1/playlists") => HandlePlaylistRequest(request),
        _ => throw new NotImplementedException()
    };
    
    private HttpClient GetMockedHttpClient()
    {
        var httpClientHandlerMock = new Mock<HttpClientHandler>();
        
        httpClientHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        ).Returns(HandleSendAsync);

        return new HttpClient(httpClientHandlerMock.Object);
    }
    
    internal static void Setup(MockableMuuzikaWebApplicationFactory factory, IConfiguration configuration)
    {
        factory.MockTransient<IHttpService>(serviceProvider =>
        {
            var mocker = new SpotifyMocker(configuration);
            var httpService = new HttpService(mocker.GetMockedHttpClient(), serviceProvider.GetRequiredService<JsonSerializerOptions>());
            mocker.HttpService = httpService;
            return httpService;
        });
    }
    
}