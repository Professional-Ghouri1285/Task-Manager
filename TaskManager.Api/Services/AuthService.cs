using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly JwtSettings _jwtSettings;

    public AuthService(AppDbContext context, IOptions<JwtSettings> jwtOptions)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<User>();
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExists)
            return AuthResult.Fail("Email already registered");

        var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == request.OrganizationId);
        if (organization == null)
            return AuthResult.Fail("Organization not found");

        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            Name = request.Name,
            Email = request.Email,
            JobTitle = request.JobTitle,
            Role = UserRole.Member,
            PasswordHash = string.Empty // placeholder, will be hashed below
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var response = new AuthResponse
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            OrganizationId = user.OrganizationId,
            Token = GenerateJwtToken(user)
        };

        return AuthResult.Ok(response);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return AuthResult.Fail("Invalid credentials");

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
            return AuthResult.Fail("Invalid credentials");

        var response = new AuthResponse
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            OrganizationId = user.OrganizationId,
            Token = GenerateJwtToken(user)
        };

        return AuthResult.Ok(response);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("organization_id", user.OrganizationId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; }
}