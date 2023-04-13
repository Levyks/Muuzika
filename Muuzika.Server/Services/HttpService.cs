using System.Net.Http.Headers;
using System.Text.Json;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Services;

public class HttpService: IHttpService
{
    private readonly HttpClient _httpClient;
    public JsonSerializerOptions JsonSerializerOptions { get; set; }

    public Uri? BaseAddress
    {
        get => _httpClient.BaseAddress;
        set => _httpClient.BaseAddress = value;
    }

    public HttpService(HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions)
    {
        _httpClient = httpClient;
        JsonSerializerOptions = jsonSerializerOptions;
    }
    
    public HttpService(JsonSerializerOptions jsonSerializerOptions): this(new HttpClient(), jsonSerializerOptions)
    {
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string url, HttpContent? requestContent, Dictionary<string, string>? headers)
    {
        using var request = new HttpRequestMessage(method, url);

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = requestContent;
            
        if (headers != null)
        {
            foreach (var (key, value) in headers)
            {
                request.Headers.Add(key, value);
            }
        }
        
        var response = await _httpClient.SendAsync(request);
        
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        
        return JsonSerializer.Deserialize<T>(responseContent, JsonSerializerOptions)!;
    }

    public Task<T> GetAsync<T>(string url, Dictionary<string, string>? headers = null)
    {
        return SendAsync<T>(HttpMethod.Get, url, null, headers);
    }

    public Task<T> PostAsync<T>(string url, HttpContent? requestContent, Dictionary<string, string>? headers = null)
    {
        return SendAsync<T>(HttpMethod.Post, url, requestContent, headers);
    }

    public Task<T> PutAsync<T>(string url, HttpContent? requestContent, Dictionary<string, string>? headers = null)
    {
        return SendAsync<T>(HttpMethod.Put, url, requestContent, headers);
    }

    public Task<T> DeleteAsync<T>(string url, Dictionary<string, string>? headers = null)
    {
        return SendAsync<T>(HttpMethod.Delete, url, null, headers);
    }
}