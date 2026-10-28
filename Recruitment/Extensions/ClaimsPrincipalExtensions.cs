using System.Security.Claims;

namespace Recruitment.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user.FindFirstValue("sub");

        return Guid.TryParse(claim, out var id) ? id : null;
    }
}