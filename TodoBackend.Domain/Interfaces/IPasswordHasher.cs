namespace TodoBackend.Domain.Interfaces;

/// <summary>
/// Password hashing ve verification i?lemleri için interface
/// Implementation Infrastructure katman?nda yap?lacak
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Password'u hash'leyip salt ile birlikte döner
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Tuple(hash, salt)</returns>
    (string hash, string salt) Hash(string password);

    /// <summary>
    /// Verilen password'un hash ve salt ile e?le?ip e?le?medi?ini kontrol eder
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="hash">Stored password hash</param>
    /// <param name="salt">Stored salt</param>
    /// <returns>True if password matches</returns>
    bool Verify(string password, string hash, string salt);
}