using System.Security.Claims;

namespace Core.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static List<string> Claims(this ClaimsPrincipal claimsPrincipal, string claimType)
        {
            var result = claimsPrincipal?.FindAll(claimType)?.Select(x => x.Value).ToList();
            return result;
        }

        public static List<string> ClaimRoles(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal?.Claims(ClaimTypes.Role);
        }

        /// <summary>
        /// Token'daki NameIdentifier claim'inden kullanıcı ID'sini döner. Claim yoksa veya geçersizse null.
        /// </summary>
        public static int? GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            var value = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out var userId) ? userId : null;
        }

        /// <summary>
        /// Kullanıcının virgülle ayrılmış rollerden en az birine sahip olup olmadığını kontrol eder.
        /// </summary>
        public static bool IsInAnyRole(this ClaimsPrincipal claimsPrincipal, string roles)
        {
            if (claimsPrincipal == null || string.IsNullOrWhiteSpace(roles))
                return false;

            return roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(claimsPrincipal.IsInRole);
        }
    }
}
