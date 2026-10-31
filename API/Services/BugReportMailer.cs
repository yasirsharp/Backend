using System.Net;
using System.Net.Mail;
using System.Text;
using Entity.Concrete;

namespace API.Services
{
    /// <summary>
    /// Yeni hata bildirimini yöneticilere e-postayla iletir. SMTP ayarı (Email:Smtp:Host) veya alıcı
    /// (BugReports:NotifyEmails) yoksa sessizce atlar; bildirim veritabanında yine kayıtlıdır.
    /// Gönderim arka planda yapılır, isteği bekletmez.
    /// </summary>
    public class BugReportMailer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BugReportMailer> _logger;

        public BugReportMailer(IConfiguration configuration, ILogger<BugReportMailer> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_configuration["Email:Smtp:Host"]) && Recipients().Length > 0;

        private string[] Recipients() =>
            (_configuration["BugReports:NotifyEmails"] ?? "")
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        public void SendInBackground(BugReport report, string adminUrl)
        {
            if (!IsConfigured)
                return;
            _ = Task.Run(async () =>
            {
                try
                {
                    await SendAsync(report, adminUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Hata bildirimi #{Id} e-postası gönderilemedi", report.Id);
                }
            });
        }

        public async Task SendAsync(BugReport report, string adminUrl)
        {
            var smtp = _configuration.GetSection("Email:Smtp");
            var host = smtp["Host"]!;
            var from = smtp["From"] ?? smtp["User"] ?? throw new InvalidOperationException("Email:Smtp:From ayarı yok.");

            using var message = new MailMessage
            {
                From = new MailAddress(from, "Sınav Takvimi"),
                Subject = $"[Sınav Takvimi] Hata bildirimi #{report.Id}: {Shorten(report.Description, 60)}",
                Body = BuildBody(report, adminUrl),
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8,
            };
            foreach (var to in Recipients())
                message.To.Add(to);
            if (report.Screenshot != null)
            {
                var ext = report.ScreenshotContentType == "image/png" ? "png" : "jpg";
                message.Attachments.Add(new Attachment(new MemoryStream(report.Screenshot), $"hata-{report.Id}.{ext}",
                    report.ScreenshotContentType ?? "image/jpeg"));
            }

            using var client = new SmtpClient(host, smtp.GetValue("Port", 587))
            {
                EnableSsl = smtp.GetValue("EnableSsl", true),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30_000,
            };
            if (!string.IsNullOrEmpty(smtp["User"]))
                client.Credentials = new NetworkCredential(smtp["User"], smtp["Password"]);
            await client.SendMailAsync(message);
            _logger.LogInformation("Hata bildirimi #{Id} e-postayla iletildi", report.Id);
        }

        public static string BuildBody(BugReport report, string adminUrl) =>
            new StringBuilder()
                .AppendLine(report.Description)
                .AppendLine()
                .AppendLine("---- Ayrıntılar ----")
                .AppendLine($"Bildiren: {report.UserName} ({report.Roles})")
                .AppendLine($"Sayfa: {report.PageUrl}")
                .AppendLine($"Sürüm: {report.AppVersion}")
                .AppendLine($"Tarayıcı: {report.UserAgent}")
                .AppendLine($"Ekran: {report.Viewport}")
                .AppendLine($"Zaman: {report.CreatedDate:dd.MM.yyyy HH:mm}")
                .AppendLine($"Ekran görüntüsü: {(report.Screenshot != null ? "ekte" : "yok")}")
                .AppendLine()
                .AppendLine($"Yönetim ekranında aç: {adminUrl}")
                .ToString();

        private static string Shorten(string text, int max)
        {
            var line = text.ReplaceLineEndings(" ");
            return line.Length <= max ? line : line[..max] + "…";
        }
    }
}
