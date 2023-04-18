using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Muuzika.Server.Services;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.ServerTests.E2E.Helpers.Mockers;

public class SpotifyMocker
{
    
    private readonly IConfiguration _configuration;
    
    private SpotifyMocker(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private HttpResponseMessage HandleTokenRequest(HttpRequestMessage request)
    {
        var body = request.Content?.ReadAsStringAsync().Result;
        
        
        throw new NotImplementedException();
        
        
    }
    
    private HttpResponseMessage HandleRequest(HttpRequestMessage request) => request.RequestUri?.ToString() switch
    {
        "https://accounts.spotify.com/api/token" => HandleTokenRequest(request),
        _ => throw new Exception($"Unexpected request: {request}")
    };

    private Task<HttpResponseMessage> HandleSendAsync(HttpRequestMessage request, CancellationToken _)
    {
        return Task.FromResult(HandleRequest(request));
    }
    
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
        var mocker = new SpotifyMocker(configuration);
        factory.MockTransient<IHttpService>(
            serviceProvider => new HttpService(mocker.GetMockedHttpClient(),
                serviceProvider.GetRequiredService<JsonSerializerOptions>())
        );
    }
    
}