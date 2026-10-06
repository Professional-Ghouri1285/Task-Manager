using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace TaskManager.UITests.Utils;

/// <summary>
/// Minimal HTTP client used by the UI test suite for non-UI setup operations
/// such as obtaining a valid JWT so that the "already logged in" redirect
/// test can pre-seed localStorage before the page loads.
/// </summary>
public class AuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly TestSettings _settings;

    public AuthApiClient(TestSettings settings)
    {
        _settings = settings;
        _httpClient = new HttpClient();
    }

    public async Task<AuthResponse?> LoginAsync(string email, string password)
    {
        var payload = JsonSerializer.Serialize(new { email, password });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/auth/login", content);
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AuthResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public void Dispose() => _httpClient.Dispose();
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
}
