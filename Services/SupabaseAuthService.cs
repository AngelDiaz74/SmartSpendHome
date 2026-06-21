using Blazored.LocalStorage;
using Microsoft.Extensions.Options;
using SmartSpendHome.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace SmartSpendHome.Services;

public class SupabaseAuthService : ISupabaseAuthService
{
    private const string SessionStorageKey = "smartspendhome_supabase_session";

    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly SupabaseSettings _settings;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SupabaseAuthService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        IOptions<SupabaseSettings> settings)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _settings = settings.Value;
    }

    public async Task<AuthResult> SignInAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new AuthResult
            {
                Success = false,
                Message = "Email and password are required."
            };
        }

        if (string.IsNullOrWhiteSpace(_settings.Url) ||
            string.IsNullOrWhiteSpace(_settings.AnonKey))
        {
            return new AuthResult
            {
                Success = false,
                Message = "Supabase settings are missing. Check wwwroot/appsettings.json."
            };
        }

        var requestUrl = $"{_settings.AuthUrl}/token?grant_type=password";

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

        request.Headers.Add("apikey", _settings.AnonKey);

        request.Content = JsonContent.Create(new
        {
            email = email.Trim(),
            password
        });

        var response = await _httpClient.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = TryGetErrorMessage(responseText);

            return new AuthResult
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(errorMessage)
                    ? $"Login failed. Status code: {(int)response.StatusCode}"
                    : errorMessage
            };
        }

        var authResponse = JsonSerializer.Deserialize<SupabaseAuthResponse>(
            responseText,
            _jsonOptions);

        if (authResponse?.User is null ||
            string.IsNullOrWhiteSpace(authResponse.AccessToken))
        {
            return new AuthResult
            {
                Success = false,
                Message = "Login succeeded, but Supabase did not return a valid session."
            };
        }

        var session = new SupabaseSession
        {
            AccessToken = authResponse.AccessToken,
            RefreshToken = authResponse.RefreshToken ?? string.Empty,
            TokenType = authResponse.TokenType ?? "bearer",
            ExpiresIn = authResponse.ExpiresIn,
            UserId = authResponse.User.Id,
            Email = authResponse.User.Email
        };

        await _localStorage.SetItemAsync(SessionStorageKey, session);

        return new AuthResult
        {
            Success = true,
            Message = "Login successful.",
            Session = session
        };
    }

    public async Task SignOutAsync()
    {
        await _localStorage.RemoveItemAsync(SessionStorageKey);
    }

    public async Task<bool> IsLoggedInAsync()
    {
        var session = await GetSessionAsync();

        return session is not null &&
               !string.IsNullOrWhiteSpace(session.AccessToken) &&
               session.UserId != Guid.Empty;
    }

    public async Task<SupabaseSession?> GetSessionAsync()
    {
        return await _localStorage.GetItemAsync<SupabaseSession>(SessionStorageKey);
    }

    public async Task<Guid?> GetUserIdAsync()
    {
        var session = await GetSessionAsync();

        if (session is null || session.UserId == Guid.Empty)
            return null;

        return session.UserId;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var session = await GetSessionAsync();

        return session?.AccessToken;
    }

    private string? TryGetErrorMessage(string responseText)
    {
        if (string.IsNullOrWhiteSpace(responseText))
            return null;

        try
        {
            var error = JsonSerializer.Deserialize<SupabaseErrorResponse>(
                responseText,
                _jsonOptions);

            return error?.ErrorDescription
                   ?? error?.Message
                   ?? error?.Error;
        }
        catch
        {
            return responseText;
        }
    }
}
