using System.Net.Http.Headers;
using System.Text.Json;

namespace Muuzika.Server.Services.Interfaces;

public interface IHttpService
{
    Uri? BaseAddress { get; set; }
    JsonSerializerOptions JsonSerializerOptions { get; set; }
    Task<T> GetAsync<T>(string url, Dictionary<string, string>? headers = null);
    Task<T> PostAsync<T>(string url, HttpContent? requestContent, Dictionary<string, string>? headers = null);
    Task<T> PutAsync<T>(string url, HttpContent? requestContent, Dictionary<string, string>? headers = null);
    Task<T> DeleteAsync<T>(string url, Dictionary<string, string>? headers = null);
}