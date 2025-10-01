namespace TodoBackend.Domain.Interfaces.Outside;

/// <summary>
/// Input sanitization ve validation i?lemleri için interface
/// XSS, SQL Injection ve di?er güvenlik aç?klar?n? önler
/// Clean Architecture: Domain katman?nda interface tan?m?
/// </summary>
public interface IInputSanitizer
{
    /// <summary>
    /// HTML içeri?ini güvenli hale getirir (Stored XSS prevention)
    /// Tehlikeli HTML tag'lar? ve attribute'lar? kald?r?r
    /// </summary>
    /// <param name="input">Sanitize edilecek HTML içeri?i</param>
    /// <returns>Güvenli HTML içeri?i</returns>
    string SanitizeHtml(string input);
    
    /// <summary>
    /// JavaScript injection'? önlemek için string'i temizler
    /// JavaScript karakterlerini encode eder ve tehlikeli pattern'leri kald?r?r
    /// </summary>
    /// <param name="input">Sanitize edilecek string</param>
    /// <returns>JavaScript aç?s?ndan güvenli string</returns>
    string SanitizeJavaScript(string input);
    
    /// <summary>
    /// Genel string input'u güvenli hale getirir
    /// HTML ve SQL injection kar?? koruma sa?lar
    /// </summary>
    /// <param name="input">Sanitize edilecek genel input</param>
    /// <returns>Güvenli string</returns>
    string SanitizeInput(string input);
    
    /// <summary>
    /// Email format validation ve sanitization
    /// Email format?n? do?rular ve güvenli hale getirir
    /// </summary>
    /// <param name="email">Sanitize edilecek email adresi</param>
    /// <returns>Güvenli email adresi</returns>
    /// <exception cref="ArgumentException">Geçersiz email format?</exception>
    string SanitizeEmail(string email);
    
    /// <summary>
    /// URL'leri güvenli hale getirir
    /// Tehlikeli protokolleri (javascript:, vbscript:, data:) kald?r?r
    /// </summary>
    /// <param name="url">Sanitize edilecek URL</param>
    /// <returns>Güvenli URL</returns>
    string SanitizeUrl(string url);
}