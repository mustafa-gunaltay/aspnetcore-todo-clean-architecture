using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace TodoBackend.Api.Services;

/// <summary>
/// JWT Token olu?turma ve do?rulama i?lemleri için service
/// </summary>
public interface IJwtService
{
    string GenerateToken(string userId, string email);
}

/// <summary>
/// JWT Token service implementation - Hem symmetric hem asymmetric key destekler
/// </summary>
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// JWT Token olu?turur - Kullan?c?n?n dijital kimlik kart?
    /// </summary>
    public string GenerateToken(string userId, string email)
    {
        var signingCredentials = GetSigningCredentials();

        // Token içine konacak bilgiler (Claims)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),   // Kullan?c? ID'si
            new Claim(ClaimTypes.Email, email),             // Email adresi
            new Claim("UserId", userId)                     // Ekstra UserId claim
        };

        // Token'? olu?tur
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],           // Token'? kim verdi
            audience: _configuration["Jwt:Audience"],       // Token kime verildi
            claims: claims,                                 // Token içindeki bilgiler
            expires: DateTime.UtcNow.AddHours(24),         // 24 saat geçerli (UTC kullan)
            signingCredentials: signingCredentials          // ?mza bilgisi
        );

        // Token'? string'e çevir ve döndür
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Konfigürasyona göre symmetric veya asymmetric signing credentials döndürür
    /// </summary>
    private SigningCredentials GetSigningCredentials()
    {
        var useAsymmetricKeys = _configuration.GetValue<bool>("Jwt:UseAsymmetricKeys");

        if (useAsymmetricKeys)
        {
            // Production: RSA Private Key kullan
            return GetRsaSigningCredentials();
        }
        else
        {
            // Development: Symmetric Key kullan
            return GetSymmetricSigningCredentials();
        }
    }

    /// <summary>
    /// RSA private key ile signing credentials olu?turur
    /// </summary>
    private SigningCredentials GetRsaSigningCredentials()
    {
        var privateKeyPath = _configuration["Jwt:PrivateKeyPath"];
        
        if (string.IsNullOrEmpty(privateKeyPath) || !File.Exists(privateKeyPath))
        {
            _logger.LogWarning("RSA private key not found at {PrivateKeyPath}, falling back to symmetric key", privateKeyPath);
            return GetSymmetricSigningCredentials();
        }

        try
        {
            var privateKeyContent = File.ReadAllText(privateKeyPath);
            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyContent);
            var rsaKey = new RsaSecurityKey(rsa);
            
            _logger.LogDebug("Using RSA private key for token signing");
            return new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load RSA private key from {PrivateKeyPath}, falling back to symmetric key", privateKeyPath);
            return GetSymmetricSigningCredentials();
        }
    }

    /// <summary>
    /// Symmetric key ile signing credentials olu?turur
    /// </summary>
    private SigningCredentials GetSymmetricSigningCredentials()
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "DefaultSecretKey1234567890123456");
        var securityKey = new SymmetricSecurityKey(key);
        
        _logger.LogDebug("Using symmetric key for token signing");
        return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }
}