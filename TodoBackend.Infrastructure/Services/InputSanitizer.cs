using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using TodoBackend.Domain.Interfaces.Outside;

namespace TodoBackend.Infrastructure.Services;

/// <summary>
/// XSS korumas? için input sanitization servisi
/// Clean Architecture: Infrastructure katman?nda concrete implementation
/// </summary>
public class InputSanitizer : IInputSanitizer
{
    private readonly HtmlEncoder _htmlEncoder;
    private readonly JavaScriptEncoder _jsEncoder;
    private readonly UrlEncoder _urlEncoder;
    private readonly ILogger<InputSanitizer> _logger;

    // Tehlikeli HTML tag'lar? ve attribute'lar?
    private static readonly HashSet<string> DangerousTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "script", "iframe", "object", "embed", "form", "input", "textarea", 
        "button", "link", "meta", "base", "style", "title", "applet",
        "frameset", "frame", "noframes", "noscript"
    };

    private static readonly HashSet<string> DangerousAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "onload", "onerror", "onclick", "onmouseover", "onfocus", "onblur",
        "onchange", "onsubmit", "onkeydown", "onkeyup", "onkeypress",
        "onmousedown", "onmouseup", "onmousemove", "onmouseout", "onmouseenter",
        "ondblclick", "oncontextmenu", "onwheel", "ondrag", "ondrop",
        "onscroll", "onresize", "onselect", "oninput", "oninvalid"
    };

    private static readonly HashSet<string> DangerousProtocols = new(StringComparer.OrdinalIgnoreCase)
    {
        "javascript:", "vbscript:", "data:", "about:", "mocha:", "livescript:"
    };

    public InputSanitizer(ILogger<InputSanitizer> logger)
    {
        _htmlEncoder = HtmlEncoder.Default;
        _jsEncoder = JavaScriptEncoder.Default;
        _urlEncoder = UrlEncoder.Default;
        _logger = logger;
    }

    public string SanitizeHtml(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            _logger.LogDebug("Empty or null HTML input received for sanitization");
            return string.Empty;
        }

        try
        {
            _logger.LogDebug("Sanitizing HTML input with length: {Length}", input.Length);
            
            var doc = new HtmlDocument();
            doc.LoadHtml(input);

            // Tehlikeli elementleri kald?r
            var removedTags = RemoveDangerousElements(doc);
            
            // Tehlikeli attribute'lar? kald?r
            var removedAttributes = RemoveDangerousAttributes(doc);

            // Tehlikeli protokolleri temizle
            CleanDangerousProtocols(doc);

            var sanitizedHtml = doc.DocumentNode.InnerHtml;
            
            if (removedTags > 0 || removedAttributes > 0)
            {
                _logger.LogWarning("HTML sanitization removed {Tags} dangerous tags and {Attributes} dangerous attributes", 
                    removedTags, removedAttributes);
            }

            return sanitizedHtml;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during HTML sanitization, falling back to HTML encoding");
            // Parse hatas? durumunda input'u encode et
            return _htmlEncoder.Encode(input);
        }
    }

    public string SanitizeJavaScript(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            _logger.LogDebug("Empty or null JavaScript input received for sanitization");
            return string.Empty;
        }

        _logger.LogDebug("Sanitizing JavaScript input with length: {Length}", input.Length);

        // JavaScript karakterlerini encode et
        var sanitized = _jsEncoder.Encode(input);
        
        // Tehlikeli pattern'leri kald?r
        var originalLength = sanitized.Length;
        
        foreach (var protocol in DangerousProtocols)
        {
            sanitized = Regex.Replace(sanitized, 
                Regex.Escape(protocol), "", 
                RegexOptions.IgnoreCase | RegexOptions.Multiline);
        }

        // eval() ve Function() gibi tehlikeli JavaScript fonksiyonlar?n? temizle
        sanitized = Regex.Replace(sanitized, @"\b(eval|Function|setTimeout|setInterval)\s*\(", 
            "", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        if (sanitized.Length != originalLength)
        {
            _logger.LogWarning("JavaScript sanitization removed dangerous patterns from input");
        }
        
        return sanitized;
    }

    public string SanitizeInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            _logger.LogDebug("Empty or null general input received for sanitization");
            return string.Empty;
        }

        _logger.LogDebug("Sanitizing general input with length: {Length}", input.Length);

        // Genel sanitization - HTML ve JS karakterlerini encode et
        var sanitized = _htmlEncoder.Encode(input);
        
        // SQL injection pattern'lerini temizle
        var originalLength = sanitized.Length;
        sanitized = Regex.Replace(sanitized, @"['""`;]", "", RegexOptions.IgnoreCase);
        
        // XSS pattern'lerini temizle
        sanitized = Regex.Replace(sanitized, @"<[^>]*>", "", RegexOptions.IgnoreCase);

        if (sanitized.Length != originalLength)
        {
            _logger.LogWarning("General input sanitization removed potentially dangerous characters");
        }
        
        return sanitized.Trim();
    }

    public string SanitizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogDebug("Empty or null email input received for sanitization");
            return string.Empty;
        }

        _logger.LogDebug("Sanitizing email: {Email}", email.Length > 20 ? email.Substring(0, 20) + "..." : email);

        // Email format kontrolü
        if (!IsValidEmail(email))
        {
            _logger.LogWarning("Invalid email format detected: {Email}", email);
            throw new ArgumentException("Invalid email format", nameof(email));
        }

        // HTML encode et ve normalize et
        var sanitized = _htmlEncoder.Encode(email.Trim().ToLowerInvariant());
        
        _logger.LogDebug("Email sanitization completed successfully");
        return sanitized;
    }

    public string SanitizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            _logger.LogDebug("Empty or null URL input received for sanitization");
            return string.Empty;
        }

        _logger.LogDebug("Sanitizing URL with length: {Length}", url.Length);

        // Tehlikeli protokolleri kontrol et
        foreach (var protocol in DangerousProtocols)
        {
            if (url.StartsWith(protocol, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Dangerous protocol detected in URL: {Protocol}", protocol);
                return string.Empty;
            }
        }

        // URL'yi encode et
        var sanitized = _urlEncoder.Encode(url);
        
        _logger.LogDebug("URL sanitization completed successfully");
        return sanitized;
    }

    private int RemoveDangerousElements(HtmlDocument doc)
    {
        var nodesToRemove = doc.DocumentNode
            .Descendants()
            .Where(node => DangerousTags.Contains(node.Name))
            .ToList();

        foreach (var node in nodesToRemove)
        {
            node.Remove();
        }

        return nodesToRemove.Count;
    }

    private int RemoveDangerousAttributes(HtmlDocument doc)
    {
        var allNodes = doc.DocumentNode.Descendants().ToList();
        var removedCount = 0;
        
        foreach (var node in allNodes)
        {
            var attributesToRemove = node.Attributes
                .Where(attr => 
                    DangerousAttributes.Contains(attr.Name) ||
                    DangerousProtocols.Any(protocol => 
                        attr.Value?.Contains(protocol, StringComparison.OrdinalIgnoreCase) == true))
                .ToList();

            foreach (var attr in attributesToRemove)
            {
                node.Attributes.Remove(attr);
                removedCount++;
            }
        }

        return removedCount;
    }

    private void CleanDangerousProtocols(HtmlDocument doc)
    {
        var allNodes = doc.DocumentNode.Descendants().ToList();
        
        foreach (var node in allNodes)
        {
            foreach (var attr in node.Attributes)
            {
                if (!string.IsNullOrEmpty(attr.Value))
                {
                    foreach (var protocol in DangerousProtocols)
                    {
                        attr.Value = attr.Value.Replace(protocol, "", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            return Regex.IsMatch(email, 
                @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", 
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}