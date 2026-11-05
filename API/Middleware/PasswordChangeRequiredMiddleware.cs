using Core.Utilities.Security.JWT;

namespace API.Middleware
{
    /// <summary>
    /// Geçici parolayla giriş yapan kullanıcı ("pwd_change" claim'i) parolasını değiştirene kadar sadece
    /// parola değiştirme ve kendi bilgilerini okuma uçlarını kullanabilir; diğer API istekleri 403 alır.
    /// </summary>
    public class PasswordChangeRequiredMiddleware
    {
        public const string ErrorCode = "PASSWORD_CHANGE_REQUIRED";

        private static readonly string[] AllowedPaths =
        {
            "/api/auth/change-password",
            "/api/auth/login",
            "/api/user/me",
        };

        private readonly RequestDelegate _next;

        public PasswordChangeRequiredMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true &&
                context.User.HasClaim(c => c.Type == CustomClaimTypes.PasswordChangeRequired) &&
                context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase) &&
                !AllowedPaths.Any(p => context.Request.Path.Equals(p, StringComparison.OrdinalIgnoreCase)))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    code = ErrorCode,
                    message = "Devam etmeden önce parolanızı değiştirmeniz gerekiyor."
                });
                return;
            }

            await _next(context);
        }
    }
}
