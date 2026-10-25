using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Dormio.Web.Services;

/// <summary>Typed HttpClient that forwards the signed-in user's JWT (stored as a cookie claim) to Dormio.Api.</summary>
public class ApiClient
{
    public const string TokenClaim = "access_token";
    private readonly HttpClient _http;

    public ApiClient(HttpClient http, IHttpContextAccessor accessor)
    {
        _http = http;
        var token = accessor.HttpContext?.User.FindFirst(TokenClaim)?.Value;
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public Task<HttpResponseMessage> PostAsync<T>(string url, T body) => _http.PostAsJsonAsync(url, body);
    public Task<HttpResponseMessage> PutAsync<T>(string url, T body) => _http.PutAsJsonAsync(url, body);
    public Task<HttpResponseMessage> DeleteAsync(string url) => _http.DeleteAsync(url);

    public async Task<T?> GetAsync<T>(string url)
    {
        var res = await _http.GetAsync(url);
        return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<T>() : default;
    }
}
