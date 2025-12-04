using System.Security.Claims;

namespace Financial.Api.Services;

public interface ICurrentUserService
{
    Guid GetUserId();
    string GetUserEmail();
    string GetUserName();
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User ID not found in claims");

        // If the userId is a GUID, return it directly; otherwise hash it to create a consistent GUID
        if (Guid.TryParse(userId, out var guidValue))
            return guidValue;

        // Create a deterministic GUID from the string userId
        return new Guid(System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(userId)));
    }

    public string GetUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value 
            ?? _httpContextAccessor.HttpContext?.User.FindFirst("email")?.Value 
            ?? "unknown@example.com";
    }

    public string GetUserName()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value 
            ?? _httpContextAccessor.HttpContext?.User.FindFirst("name")?.Value 
            ?? "Unknown User";
    }
}
