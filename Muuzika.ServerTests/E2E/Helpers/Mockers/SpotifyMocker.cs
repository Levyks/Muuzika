using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
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
    
    private async Task<HttpResponseMessage> HandlePlaylistInfoRequest(string playlistId, string? fields)
    {
        if (fields != SpotifyPlaylistInfoWithTracksFirstPageDto.Fields)
            throw new Exception($"Unexpected request for playlist info with fields: {fields}");
        
        var responseJson = await File.ReadAllTextAsync($"Fixtures/Spotify/Playlists/{playlistId}/info.json");
        
        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };
    }
    
    private async Task<HttpResponseMessage> HandleWebApiRequest(HttpRequestMessage request)
    {
        var uri = request.RequestUri!;
        
        if (uri.LocalPath.StartsWith("/v1/playlists/") && uri.Segments.Length == 4)
        {
            var parsedQuery = HttpUtility.ParseQueryString(uri.Query);
            return await HandlePlaylistInfoRequest(uri.Segments[^1], parsedQuery["fields"]);
        }
        
        var playlistId = uri.Segments[^3];

        throw new NotImplementedException();
    }

    private Task<HttpResponseMessage> HandleAccountsRequest(HttpRequestMessage request) => 
        request.RequestUri?.LocalPath switch 
        { 
            "/api/token" => HandleTokenRequest(request), 
            _ => throw new Exception($"Unexpected request: {request}") 
        };

    private Task<HttpResponseMessage> HandleSendAsync(HttpRequestMessage request, CancellationToken _) =>
        request.RequestUri?.Host switch
        {
            "accounts.spotify.com" => HandleAccountsRequest(request),
            "api.spotify.com" => HandleWebApiRequest(request),
            _ => throw new Exception($"Unexpected request: {request}")
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