using System.Security.Cryptography;

namespace TodoBackend.Api.Services;

/// <summary>
/// RSA key pair olu?turma ve yönetimi için service
/// </summary>
public interface IKeyGenerationService
{
    Task<bool> GenerateRsaKeyPairAsync();
    Task<bool> RotateKeysAsync();
    bool KeysExist();
}

/// <summary>
/// RSA key generation service implementation
/// </summary>
public class KeyGenerationService : IKeyGenerationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeyGenerationService> _logger;

    public KeyGenerationService(IConfiguration configuration, ILogger<KeyGenerationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// RSA key pair (private/public) olu?turur
    /// </summary>
    public async Task<bool> GenerateRsaKeyPairAsync()
    {
        try
        {
            using var rsa = RSA.Create(2048); // 2048-bit RSA key

            var privateKeyPath = _configuration["Jwt:PrivateKeyPath"];
            var publicKeyPath = _configuration["Jwt:PublicKeyPath"];

            if (string.IsNullOrEmpty(privateKeyPath) || string.IsNullOrEmpty(publicKeyPath))
            {
                _logger.LogError("Key paths are not configured in appsettings.json");
                return false;
            }

            // Klasör olu?tur
            Directory.CreateDirectory(Path.GetDirectoryName(privateKeyPath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(publicKeyPath)!);

            // Private key kaydet
            var privateKeyPem = rsa.ExportRSAPrivateKeyPem();
            await File.WriteAllTextAsync(privateKeyPath, privateKeyPem);

            // Public key kaydet  
            var publicKeyPem = rsa.ExportRSAPublicKeyPem();
            await File.WriteAllTextAsync(publicKeyPath, publicKeyPem);

            _logger.LogInformation("RSA key pair generated successfully at {PrivateKeyPath} and {PublicKeyPath}", 
                privateKeyPath, publicKeyPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate RSA key pair");
            return false;
        }
    }

    /// <summary>
    /// Mevcut anahtarlar? yedekleyip yeni anahtar çifti olu?turur
    /// </summary>
    public async Task<bool> RotateKeysAsync()
    {
        try
        {
            _logger.LogInformation("Starting key rotation...");
            
            // Eski anahtarlar? yedekle
            await BackupCurrentKeysAsync();
            
            // Yeni anahtar çifti olu?tur
            var result = await GenerateRsaKeyPairAsync();
            
            if (result)
            {
                _logger.LogInformation("Key rotation completed successfully");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rotate keys");
            return false;
        }
    }

    /// <summary>
    /// RSA key dosyalar?n?n mevcut olup olmad???n? kontrol eder
    /// </summary>
    public bool KeysExist()
    {
        var privateKeyPath = _configuration["Jwt:PrivateKeyPath"];
        var publicKeyPath = _configuration["Jwt:PublicKeyPath"];

        return !string.IsNullOrEmpty(privateKeyPath) && 
               !string.IsNullOrEmpty(publicKeyPath) &&
               File.Exists(privateKeyPath) && 
               File.Exists(publicKeyPath);
    }

    /// <summary>
    /// Mevcut anahtarlar? timestamp ile yedekler
    /// </summary>
    private async Task BackupCurrentKeysAsync()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var privateKeyPath = _configuration["Jwt:PrivateKeyPath"];
        var publicKeyPath = _configuration["Jwt:PublicKeyPath"];

        if (!string.IsNullOrEmpty(privateKeyPath) && File.Exists(privateKeyPath))
        {
            var backupPrivatePath = $"{privateKeyPath}.{timestamp}.backup";
            File.Copy(privateKeyPath, backupPrivatePath);
            _logger.LogInformation("Private key backed up to {BackupPath}", backupPrivatePath);
        }

        if (!string.IsNullOrEmpty(publicKeyPath) && File.Exists(publicKeyPath))
        {
            var backupPublicPath = $"{publicKeyPath}.{timestamp}.backup";
            File.Copy(publicKeyPath, backupPublicPath);
            _logger.LogInformation("Public key backed up to {BackupPath}", backupPublicPath);
        }
    }
}