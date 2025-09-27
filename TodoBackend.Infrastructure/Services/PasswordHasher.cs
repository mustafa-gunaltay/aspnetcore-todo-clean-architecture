using BCrypt.Net;
using TodoBackend.Domain.Interfaces.Outside;

namespace TodoBackend.Infrastructure.Services;

/// <summary>
/// BCrypt kullanarak password hashing ve verification i?lemlerini gerçekle?tirir
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12; // BCrypt work factor (güvenlik seviyesi)

    /// <summary>
    /// Password'u BCrypt ile hash'ler ve salt üretir
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Tuple(hash, salt)</returns>
    public (string hash, string salt) Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        // BCrypt kendi salt'?n? üretir, biz ayr?ca salt döndürürüz
        var salt = BCrypt.Net.BCrypt.GenerateSalt(WorkFactor);
        var hash = BCrypt.Net.BCrypt.HashPassword(password, salt);

        return (hash, salt);
    }

    /// <summary>
    /// Password'un hash ile e?le?ip e?le?medi?ini kontrol eder
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="hash">Stored password hash</param>
    /// <param name="salt">Stored salt (BCrypt için gerekli de?il ama consistency için)</param>
    /// <returns>True if password matches</returns>
    public bool Verify(string password, string hash, string salt)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;
        
        if (string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            // BCrypt verify - salt hash içinde embedded oldu?u için ayr?ca salt parametre olarak gerekmiyor
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false; // Herhangi bir hata durumunda false döner
        }
    }
}