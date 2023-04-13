using System.Net;
using System.Text;
using System.Text.Json;
using Moq;
using Moq.Protected;
using Muuzika.Server.Services;

namespace Muuzika.ServerTests.Unit.Services;

public class HttpServiceTests
{
    private Mock<HttpClientHandler> _httpClientHandlerMock = null!;

    private record TestDto(string TestProperty)
    {
        public HttpResponseMessage CreateHttpResponseMessage(JsonSerializerOptions? jsonSerializerOptions = null)
        {
            var responseContent = JsonSerializer.Serialize(this, jsonSerializerOptions);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };
        }
    }
    
    public enum JsonNamingPolicyEnum
    {
        CamelCase,
        SnakeCaseLower,
        SnakeCaseUpper,
        KebabCaseLower,
        KebabCaseUpper
    }
    
    private JsonNamingPolicy GetJsonNamingPolicy(JsonNamingPolicyEnum jsonNamingPolicy)
    {
        return jsonNamingPolicy switch
        {
            JsonNamingPolicyEnum.CamelCase => JsonNamingPolicy.CamelCase,
            JsonNamingPolicyEnum.SnakeCaseLower => JsonNamingPolicy.SnakeCaseLower,
            JsonNamingPolicyEnum.SnakeCaseUpper => JsonNamingPolicy.SnakeCaseUpper,
            JsonNamingPolicyEnum.KebabCaseLower => JsonNamingPolicy.KebabCaseLower,
            JsonNamingPolicyEnum.KebabCaseUpper => JsonNamingPolicy.KebabCaseUpper,
            _ => throw new ArgumentOutOfRangeException(nameof(jsonNamingPolicy), jsonNamingPolicy, null)
        };
    }
    
    private async Task<(T, HttpRequestMessage)> MockAndDoRequest<T>(HttpResponseMessage expectedResponse, Func<HttpService, Task<T>> doRequest)
    {
        HttpRequestMessage? passedRequestMessage = null;
        _httpClientHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((requestMessage, _) => passedRequestMessage = requestMessage)
            .ReturnsAsync(expectedResponse);
        
        var httpService = new HttpService(new HttpClient(_httpClientHandlerMock.Object), new JsonSerializerOptions());
        
        var response = await doRequest(httpService);
        
        _httpClientHandlerMock.Protected()
            .Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        
        Assert.That(passedRequestMessage, Is.Not.Null);
        
        return (response, passedRequestMessage!);
    }
    
    [SetUp]
    public void Setup()
    {
        _httpClientHandlerMock = new Mock<HttpClientHandler>();
    }

    [Test]
    public async Task GetAsync_ReturnsResponseWhenStatusIsOk()
    {
        var expectedResponse = new TestDto("42");

        const string url = "https://fake-url.com/";
        var (actualResponse, passedRequestMessage) =
            await MockAndDoRequest(expectedResponse.CreateHttpResponseMessage(), httpService => httpService.GetAsync<TestDto>(url));
        
        Assert.Multiple(() =>
        {
            Assert.That(passedRequestMessage.Method, Is.EqualTo(HttpMethod.Get));
            Assert.That(passedRequestMessage.RequestUri?.ToString(), Is.EqualTo(url));
            Assert.That(passedRequestMessage.Headers.Accept, Has.Count.EqualTo(1));
            Assert.That(passedRequestMessage.Headers.Accept.First().MediaType, Is.EqualTo("application/json"));
            Assert.That(passedRequestMessage.Content, Is.Null);
        });
        
        Assert.That(actualResponse, Is.EqualTo(expectedResponse));
    }

    [Test]
    public async Task GetAsync_ThrowsExceptionWhenStatusIsNotOk()
    {
        var expectedResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);

        const string url = "https://fake-url.com/";
        try
        {
            await MockAndDoRequest(expectedResponseMessage, httpService => httpService.GetAsync<TestDto>(url));
            Assert.Fail("Expected exception was not thrown.");
        }
        catch (HttpRequestException exception)
        {
            Assert.That(exception.Message, Is.EqualTo("Response status code does not indicate success: 400 (Bad Request)."));
        }
    }
    
    [Test]
    public async Task GetAsync_UsesBaseAddress()
    {
        var expectedResponse = new TestDto("42");

        const string url = "https://fake-url.com";
        const string path = "/test";
        var (_, passedRequestMessage) =
            await MockAndDoRequest(expectedResponse.CreateHttpResponseMessage(), httpService =>
            {
                var uri = new Uri(url);
                httpService.BaseAddress = uri;
                Assert.That(httpService.BaseAddress, Is.EqualTo(uri));
                return httpService.GetAsync<TestDto>(path);
            });
        
        Assert.Multiple(() =>
        {
            Assert.That(passedRequestMessage.RequestUri?.ToString(), Is.EqualTo(url + path));
        });
    }

    [Test]
    [TestCase(JsonNamingPolicyEnum.CamelCase)]
    [TestCase(JsonNamingPolicyEnum.SnakeCaseLower)]
    public async Task GetAsync_UsesJsonSerializerOptions(JsonNamingPolicyEnum jsonNamingPolicyEnum)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = GetJsonNamingPolicy(jsonNamingPolicyEnum)
        };
        
        var expectedResponse = new TestDto("42");

        const string url = "https://fake-url.com/";
        var (actualResponse, _) =
            await MockAndDoRequest(expectedResponse.CreateHttpResponseMessage(jsonSerializerOptions), httpService =>
            {
                httpService.JsonSerializerOptions = jsonSerializerOptions;
                return httpService.GetAsync<TestDto>(url);
            });
        
        Assert.That(actualResponse, Is.EqualTo(expectedResponse));
    }
    
    [Test]
    public async Task GetAsync_UsesHeaders()
    {
        var expectedResponse = new TestDto("42");

        const string url = "https://fake-url.com/";
        const string headerKey = "TestHeader";
        const string headerValue = "TestValue";
        var (_, passedRequestMessage) =
            await MockAndDoRequest(expectedResponse.CreateHttpResponseMessage(), httpService =>
            {
                var headers = new Dictionary<string, string>
                {
                    {headerKey, headerValue}
                };
                return httpService.GetAsync<TestDto>(url, headers);
            });
        
        Assert.Multiple(() =>
        {
            Assert.That(passedRequestMessage.Headers.GetValues(headerKey).ToList(), Has.Count.EqualTo(1));
            Assert.That(passedRequestMessage.Headers.GetValues(headerKey).First(), Is.EqualTo(headerValue));
        });
    }
    
    [Test]
    public async Task PostAsync_ReturnsResponseWhenStatusIsOk()
    {
        var expectedResponse = new TestDto("42");

        const string url = "https://fake-url.com/";
        var (actualResponse, passedRequestMessage) =
            await MockAndDoRequest(expectedResponse.CreateHttpResponseMessage(), httpService => httpService.PostAsync<TestDto>(url, null));
        
        Assert.Multiple(() =>
        {
            Assert.That(passedRequestMessage.Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(passedRequestMessage.RequestUri?.ToString(), Is.EqualTo(url));
            Assert.That(passedRequestMessage.Headers.Accept, Has.Count.EqualTo(1));
            Assert.That(passedRequestMessage.Headers.Accept.First().MediaType, Is.EqualTo("application/json"));
            Assert.That(passedRequestMessage.Content, Is.Null);
        });
        
        Assert.That(actualResponse, Is.EqualTo(expectedResponse));
    }

    [Test]
    public async Task PostAsync_SendsContentInRequest()
    {
        var expectedContentDto = new TestDto("forty-two");
        var expectedContent = new StringContent(JsonSerializer.Serialize(expectedContentDto), Encoding.UTF8,
            "application/json");

        var expectedResponse = new TestDto("42");

        const string url = "https://fake-url.com/";
        var (_, passedRequestMessage) =
            await MockAndDoRequest(expectedResponse.CreateHttpResponseMessage(),
                httpService => httpService.PostAsync<TestDto>(url, expectedContent));

        Assert.That(passedRequestMessage.Content, Is.EqualTo(expectedContent));
    }
    
    [Test]
    public async Task PutAsync_ReturnsResponseWhenStatusIsOk()
    {
        var expectedResponse = new TestDto("42");

        const string url = "https://fake-url.com/";
        var (actualResponse, passedRequestMessage) =
            await MockAndDoRequest(expectedResponse.CreateHttpResponseMessage(), httpService => httpService.PutAsync<TestDto>(url, null));
        
        Assert.Multiple(() =>
        {
            Assert.That(passedRequestMessage.Method, Is.EqualTo(HttpMethod.Put));
            Assert.That(passedRequestMessage.RequestUri?.ToString(), Is.EqualTo(url));
            Assert.That(passedRequestMessage.Headers.Accept, Has.Count.EqualTo(1));
            Assert.That(passedRequestMessage.Headers.Accept.First().MediaType, Is.EqualTo("application/json"));
            Assert.That(passedRequestMessage.Content, Is.Null);
        });
        
        Assert.That(actualResponse, Is.EqualTo(expectedResponse));
    }
    
    [Test]
    public async Task DeleteAsync_ReturnsResponseWhenStatusIsOk()
    {
        var expectedResponse = new TestDto("42");

        const string url = "https://fake-url.com/";
        var (actualResponse, passedRequestMessage) =
            await MockAndDoRequest(expectedResponse.CreateHttpResponseMessage(), httpService => httpService.DeleteAsync<TestDto>(url));
        
        Assert.Multiple(() =>
        {
            Assert.That(passedRequestMessage.Method, Is.EqualTo(HttpMethod.Delete));
            Assert.That(passedRequestMessage.RequestUri?.ToString(), Is.EqualTo(url));
            Assert.That(passedRequestMessage.Headers.Accept, Has.Count.EqualTo(1));
            Assert.That(passedRequestMessage.Headers.Accept.First().MediaType, Is.EqualTo("application/json"));
            Assert.That(passedRequestMessage.Content, Is.Null);
        });
        
        Assert.That(actualResponse, Is.EqualTo(expectedResponse));
    }
}