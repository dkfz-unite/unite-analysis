using System.Security.Claims;

namespace Unite.Analysis.Web.Extensions;

public static class PersonalizationExtensions
{
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (String.IsNullOrEmpty(userIdClaim))
            return null;

        if (!int.TryParse(userIdClaim, out var userId))
            return null;

        return userId;
    }

    public static bool GetIsRoot(this ClaimsPrincipal user)
    {
        var roleClaim = user.FindFirstValue(ClaimTypes.Role);
        return !String.IsNullOrEmpty(roleClaim) && roleClaim == "Root";
    }
}
