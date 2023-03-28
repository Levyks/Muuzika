using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace Muuzika.ServerTests.E2E;

public static class HttpClientExtensions
{
    public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, [StringSyntax("Uri")] string? requestUri, object body)
    {
        var serializedBody = JsonSerializer.Serialize(body);
        var content = new StringContent(serializedBody, Encoding.UTF8, "application/json");
        return client.PostAsync("/room", content);
    }
}