using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Claims;
using Business.Abstract;
using Business.Helpers;
using Core.Extensions;
using Core.Utilities.Reporting;
using Entity.Concrete;

namespace API.Services
{
    /// <summary>
    /// Beklenmeyen sunucu hatalarını hata bildirimlerine kendiliğinden kaydeder. Aynı hata (tür + kod yeri + yol)
    /// tekrar ederse yeni kayıt açılmaz, sayacı artar; e-posta yalnızca ilk seferde gider.
    /// İstek gövdesi ve sorgu dizesi kaydedilmez (kişisel veri girmesin). Asla istisna fırlatmaz.
    /// </summary>
    public class AutoBugReporter : IUnhandledExceptionReporter
    {
        private const int MaxStackChars = 8000;
        /// <summary>Hata fırtınasında veritabanını yormamak için aynı hata en fazla saniyede bir yazılır</summary>
        private static readonly TimeSpan Throttle = TimeSpan.FromSeconds(1);

        private readonly IServiceProvider _services;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly BugReportMailer _mailer;
        private readonly ILogger<AutoBugReporter> _logger;
        private readonly ConcurrentDictionary<string, DateTime> _lastWrite = new();
        private static readonly string ServerVersion =
            Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";

        public AutoBugReporter(IServiceProvider services, IHttpContextAccessor httpContextAccessor, BugReportMailer mailer, ILogger<AutoBugReporter> logger)
        {
            _services = services;
            _httpContextAccessor = httpContextAccessor;
            _mailer = mailer;
            _logger = logger;
        }

        public void Report(Exception exception, string? context = null)
        {
            try
            {
                var http = _httpContextAccessor.HttpContext;
                var root = exception.GetBaseException();
                var path = BugReportRules.NormalizePath(http?.Request.Path.Value);
                var method = http?.Request.Method ?? "";
                var fingerprint = BugReportRules.Fingerprint(BugReportSources.Sunucu, root.GetType().FullName,
                    BugReportRules.AppFrame(root), method, path, context);

                var now = DateTime.UtcNow;
                if (_lastWrite.TryGetValue(fingerprint, out var last) && now - last < Throttle)
                    return;
                _lastWrite[fingerprint] = now;

                var user = http?.User;
                var name = user?.FindFirst(ClaimTypes.Name)?.Value;
                var email = user?.FindFirst(ClaimTypes.Email)?.Value ?? user?.FindFirst("email")?.Value;
                var actAs = user?.FindFirst(Business.Concrete.AuthManager.ActAsByNameClaim)?.Value;
                if (actAs != null) name = $"{name} ({actAs} adına)";
                var stack = exception.ToString();
                var report = new BugReport
                {
                    UserId = user?.GetUserId() ?? 0,
                    UserName = string.Join(" · ", new[] { name, email }.Where(x => !string.IsNullOrWhiteSpace(x))) is { Length: > 0 } who ? who : "(oturum yok)",
                    Roles = string.Join(", ", user?.ClaimRoles() ?? new List<string>()),
                    Description = $"Sunucu hatası{(string.IsNullOrWhiteSpace(context) ? "" : $" ({context})")}: {root.GetType().Name}: {root.Message}",
                    PageUrl = $"{method} {path}".Trim(),
                    AppVersion = $"sunucu {ServerVersion}".Trim(),
                    UserAgent = http?.Request.Headers.UserAgent.ToString() ?? "",
                    Viewport = "",
                    ClientLog = stack.Length > MaxStackChars ? stack[..MaxStackChars] : stack,
                    Source = BugReportSources.Sunucu,
                    Fingerprint = fingerprint,
                };

                var service = _services.GetRequiredService<IBugReportService>();
                var result = service.RecordAutomatic(report);
                if (result.Success && result.Data.IsNew && http != null)
                    _mailer.SendInBackground(report, $"{http.Request.Scheme}://{http.Request.Host}/yonetim/hata-bildirimleri?id={result.Data.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Beklenmeyen hata, hata bildirimlerine kaydedilemedi");
            }
        }
    }
}
