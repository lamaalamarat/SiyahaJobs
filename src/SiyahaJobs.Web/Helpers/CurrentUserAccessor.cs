using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SiyahaJobs.Web.Helpers;

/// <summary>
/// Small abstraction over HttpContext to access the current authenticated user
/// from services/repositories without coupling them to HttpContext directly.
/// </summary>
public class CurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) =>
        _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
}
