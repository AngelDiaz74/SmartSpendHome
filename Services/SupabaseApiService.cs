using Microsoft.Extensions.Options;
using SmartSpendHome.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartSpendHome.Services;

public class SupabaseApiService : ISupabaseApiService
{
    private readonly HttpClient _httpClient;
    private readonly ISupabaseAuthService _authService;
    private readonly SupabaseSettings _settings;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public SupabaseApiService(
        HttpClient httpClient,
        ISupabaseAuthService authService,
        IOptions<SupabaseSettings> settings)
    {
        _httpClient = httpClient;
        _authService = authService;
        _settings = settings.Value;
    }

    public async Task<List<T>> GetListAsync<T>(string relativeUrl)
    {
        using var request = await CreateRequestAsync(HttpMethod.Get, relativeUrl);

        var response = await _httpClient.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(GetErrorMessage(response.StatusCode, responseText));

        var result = JsonSerializer.Deserialize<List<T>>(responseText, _jsonOptions);

        return result ?? new List<T>();
    }

    public async Task<List<TResponse>> InsertAsync<TRequest, TResponse>(
        string relativeUrl,
        TRequest body)
    {
        using var request = await CreateRequestAsync(HttpMethod.Post, relativeUrl);

        request.Headers.Add("Prefer", "return=representation");
        request.Content = CreateJsonContent(body);

        var response = await _httpClient.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(GetErrorMessage(response.StatusCode, responseText));

        var result = JsonSerializer.Deserialize<List<TResponse>>(responseText, _jsonOptions);

        return result ?? new List<TResponse>();
    }

    public async Task UpdateAsync<TRequest>(string relativeUrl, TRequest body)
    {
        using var request = await CreateRequestAsync(HttpMethod.Patch, relativeUrl);

        request.Headers.Add("Prefer", "return=minimal");
        request.Content = CreateJsonContent(body);

        var response = await _httpClient.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(GetErrorMessage(response.StatusCode, responseText));
    }

    public async Task DeleteAsync(string relativeUrl)
    {
        using var request = await CreateRequestAsync(HttpMethod.Delete, relativeUrl);

        var response = await _httpClient.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(GetErrorMessage(response.StatusCode, responseText));
    }

    private async Task<HttpRequestMessage> CreateRequestAsync(
        HttpMethod method,
        string relativeUrl)
    {
        var accessToken = await _authService.GetAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new InvalidOperationException("You are not logged in. Please login first.");

        var url = $"{_settings.RestUrl.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";

        var request = new HttpRequestMessage(method, url);

        request.Headers.Add("apikey", _settings.AnonKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return request;
    }

    private StringContent CreateJsonContent<T>(T body)
    {
        var json = JsonSerializer.Serialize(body, _jsonOptions);

        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private string GetErrorMessage(System.Net.HttpStatusCode statusCode, string responseText)
    {
        if (string.IsNullOrWhiteSpace(responseText))
            return $"Supabase request failed. Status code: {(int)statusCode}";

        return $"Supabase request failed. Status code: {(int)statusCode}. Response: {responseText}";
    }
}
