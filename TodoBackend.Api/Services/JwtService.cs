using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TodoBackend.Api.Services;

/// <summary>
/// JWT Token olu?turma ve do?rulama i?lemleri i�in service
/// </summary>
public interface IJwtService
{
    string GenerateToken(string userId, string email);
}

/// <summary>
/// JWT Token service implementation
/// </summary>
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// JWT Token olu?turur - Kullan?c?n?n dijital kimlik kart?
    /// </summary>
    public string GenerateToken(string userId, string email)
    {
        // 1. Gizli anahtar (appsettings.json'dan al?n?r)
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "DefaultSecretKey1234567890123456");
        var securityKey = new SymmetricSecurityKey(key);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // 2. Token i�ine konacak bilgiler (Claims)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),   // Kullan?c? ID'si
            new Claim(ClaimTypes.Email, email),             // Email adresi
            new Claim("UserId", userId)                     // Ekstra UserId claim
        };

        // 3. Token'? olu?tur
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],           // Token'? kim verdi
            audience: _configuration["Jwt:Audience"],       // Token kime verildi
            claims: claims,                                 // Token i�indeki bilgiler
            expires: DateTime.Now.AddHours(24),            // 24 saat ge�erli
            signingCredentials: credentials                 // ?mza bilgisi
        );

        // 4. Token'? string'e �evir ve d�nd�r
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}