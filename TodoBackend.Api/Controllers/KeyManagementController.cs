using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoBackend.Api.Services;

namespace TodoBackend.Api.Controllers;

/// <summary>
/// RSA key yönetimi için controller - Sadece development ortam?nda kullan?l?r
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class KeyManagementController : ControllerBase
{
    private readonly IKeyGenerationService _keyGenerationService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeyManagementController> _logger;

    public KeyManagementController(
        IKeyGenerationService keyGenerationService, 
        IConfiguration configuration,
        ILogger<KeyManagementController> logger)
    {
        _keyGenerationService = keyGenerationService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// RSA key pair olu?turur - Sadece development ortam?nda
    /// </summary>
    [HttpPost("generate-keys")]
    [AllowAnonymous] // Development için anonymous eri?im
    public async Task<IActionResult> GenerateKeys()
    {
        // Sadece development ortam?nda çal??t?r
        if (!IsDevelopmentEnvironment())
        {
            return BadRequest(new { message = "This endpoint is only available in development environment" });
        }

        try
        {
            var result = await _keyGenerationService.GenerateRsaKeyPairAsync();
            
            if (result)
            {
                _logger.LogInformation("RSA keys generated via API endpoint");
                return Ok(new { 
                    message = "RSA key pair generated successfully",
                    privateKeyPath = _configuration["Jwt:PrivateKeyPath"],
                    publicKeyPath = _configuration["Jwt:PublicKeyPath"]
                });
            }
            else
            {
                return BadRequest(new { message = "Failed to generate RSA key pair" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating RSA keys via API");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Mevcut anahtarlari rotate eder - Sadece development ortaminda
    /// Mevcut 
    /// </summary>
    [HttpPost("rotate-keys")]
    [AllowAnonymous] // Development için anonymous eri?im
    public async Task<IActionResult> RotateKeys()
    {
        // Sadece development ortam?nda çal??t?r
        if (!IsDevelopmentEnvironment())
        {
            return BadRequest(new { message = "This endpoint is only available in development environment" });
        }

        try
        {
            var result = await _keyGenerationService.RotateKeysAsync();
            
            if (result)
            {
                _logger.LogInformation("RSA keys rotated via API endpoint");
                return Ok(new { message = "Keys rotated successfully" });
            }
            else
            {
                return BadRequest(new { message = "Failed to rotate keys" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating RSA keys via API");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Key durumunu kontrol eder
    /// </summary>
    [HttpGet("key-status")]
    [AllowAnonymous] // Development için anonymous eri?im
    public IActionResult GetKeyStatus()
    {
        // Sadece development ortam?nda çal??t?r
        if (!IsDevelopmentEnvironment())
        {
            return BadRequest(new { message = "This endpoint is only available in development environment" });
        }

        var useAsymmetricKeys = _configuration.GetValue<bool>("Jwt:UseAsymmetricKeys");
        var keysExist = _keyGenerationService.KeysExist();
        
        return Ok(new { 
            useAsymmetricKeys = useAsymmetricKeys,
            keysExist = keysExist,
            privateKeyPath = _configuration["Jwt:PrivateKeyPath"],
            publicKeyPath = _configuration["Jwt:PublicKeyPath"]
        });
    }

    /// <summary>
    /// Development ortam?nda olup olmad???n? kontrol eder
    /// </summary>
    private bool IsDevelopmentEnvironment()
    {
        var environment = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") ?? 
                         Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? 
                         "Production";
        
        return environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
    }
}