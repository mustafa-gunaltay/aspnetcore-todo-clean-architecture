using System.Security.Claims;
using TodoBackend.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace TodoBackend.Api.Configs;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // suan authentication olmadigi icin default "system" donuyor
    public string UserName => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
}
