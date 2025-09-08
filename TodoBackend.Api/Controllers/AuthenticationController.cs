using MediatR;
using Microsoft.AspNetCore.Mvc;
using TodoBackend.Application.Features.Authentication.Login;
using TodoBackend.Api.Services;

namespace TodoBackend.Api.Controllers;

/// <summary>
/// Authentication (Kimlik do?rulama) i?lemleri için controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IJwtService _jwtService;

    public AuthenticationController(IMediator mediator, IJwtService jwtService)
    {
        _mediator = mediator;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Kullan?c? giri?i - Email/password ile JWT token al?r
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        
        if (result.IsSuccess)
        {
            // User bilgilerini parse et
            var userInfo = result.Value.Split('|');
            var userId = userInfo[0];
            var email = userInfo[1];

            // JWT Token olu?tur
            var token = _jwtService.GenerateToken(userId, email);

            // Token'? döndür
            return Ok(new { 
                isSuccess = true, 
                value = token, 
                successes = new[] { "Login successful" } 
            });
        }

        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);

        // Invalid credentials için 401 Unauthorized
        if (result.Errors.Any(e => e.Contains("Invalid email or password")))
            return Unauthorized(result);

        // Other errors için 400 Bad Request
        return BadRequest(result);
    }
}