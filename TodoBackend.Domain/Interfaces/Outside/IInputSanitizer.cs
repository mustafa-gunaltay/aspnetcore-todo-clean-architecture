namespace TodoBackend.Domain.Interfaces.Outside;

/// <summary>
/// Input sanitization ve validation i?lemleri i�in interface
/// XSS, SQL Injection ve di?er g�venlik a�?klar?n? �nler
/// Clean Architecture: Domain katman?nda interface tan?m?
/// </summary>
public interface IInputSanitizer
{
    /// <summary>
    /// HTML i�eri?ini g�venli hale getirir (Stored XSS prevention)
    /// Tehlikeli HTML tag'lar? ve attribute'lar? kald?r?r
    /// </summary>
    /// <param name="input">Sanitize edilecek HTML i�eri?i</param>
    /// <returns>G�venli HTML i�eri?i</returns>
    string SanitizeHtml(string input);
    
    /// <summary>
    /// JavaScript injection'? �nlemek i�in string'i temizler
    /// JavaScript karakterlerini encode eder ve tehlikeli pattern'leri kald?r?r
    /// </summary>
    /// <param name="input">Sanitize edilecek string</param>
    /// <returns>JavaScript a�?s?ndan g�venli string</returns>
    string SanitizeJavaScript(string input);
    
    /// <summary>
    /// Genel string input'u g�venli hale getirir
    /// HTML ve SQL injection kar?? koruma sa?lar
    /// </summary>
    /// <param name="input">Sanitize edilecek genel input</param>
    /// <returns>G�venli string</returns>
    string SanitizeInput(string input);
    
    /// <summary>
    /// Email format validation ve sanitization
    /// Email format?n? do?rular ve g�venli hale getirir
    /// </summary>
    /// <param name="email">Sanitize edilecek email adresi</param>
    /// <returns>G�venli email adresi</returns>
    /// <exception cref="ArgumentException">Ge�ersiz email format?</exception>
    string SanitizeEmail(string email);
    
    /// <summary>
    /// URL'leri g�venli hale getirir
    /// Tehlikeli protokolleri (javascript:, vbscript:, data:) kald?r?r
    /// </summary>
    /// <param name="url">Sanitize edilecek URL</param>
    /// <returns>G�venli URL</returns>
    string SanitizeUrl(string url);
}