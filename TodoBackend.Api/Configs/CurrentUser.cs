using System.Security.Claims;
using TodoBackend.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace TodoBackend.Api.Configs;

/// <summary>
/// Şu anki giriş yapmış kullanıcının bilgilerini verir
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Giriş yapmış kullanıcının email adresini döner
    /// Eğer giriş yapmamışsa "system" döner
    /// </summary>
    public string UserName
    {
        get
        {
            // JWT Token'dan email bilgisini al
            var email = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
            
            // Eğer email yoksa, eski yöntemle NameIdentifier'ı dene
            if (string.IsNullOrEmpty(email))
            {
                email = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            
            // Hiçbiri yoksa "system" döndür (authentication olmayan durumlar için)
            return email ?? "system";
        }
    }

    /// <summary>
    /// Giriş yapmış kullanıcının ID'sini döner
    /// </summary>
    public int? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            
            return null;
        }
    }
}
