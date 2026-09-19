using TaskManager.Api.DTOs;

namespace TaskManager.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> LoginAsync(LoginRequest request);
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public AuthResponse? Response { get; set; }

    public static AuthResult Ok(AuthResponse response) => new() { Success = true, Response = response };
    public static AuthResult Fail(string error) => new() { Success = false, Error = error };
}