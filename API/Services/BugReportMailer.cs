using System.Net.Sockets;
using System.Text;
using Entity.Concrete;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace API.Services
{
    /// <summary>
    /// Yeni hata bildirimini yöneticilere e-postayla iletir. SMTP ayarı (Email:Smtp:Host) veya alıcı
    /// (BugReports:NotifyEmails) yoksa sessizce atlar; bildirim veritabanında yine kayıtlıdır.
    /// Gönderim arka planda yapılır, isteği bekletmez.
    /// MailKit kullanılır: .NET'in SmtpClient'ı 465 portundaki doğrudan SSL'i (SMTPS) desteklemez.
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

        public string[] Recipients() =>
            (_configuration["BugReports:NotifyEmails"] ?? "")
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        /// <summary>
        /// Email:Smtp:Security = Auto (varsayılan) | SslOnConnect | StartTls | None.
        /// Auto: 465 → doğrudan SSL; EnableSsl=false → şifresiz; diğer → sunucu destekliyorsa STARTTLS.
        /// </summary>
        public static SecureSocketOptions ResolveSecurity(IConfiguration smtp)
        {
            var setting = (smtp["Security"] ?? "Auto").Trim();
            if (setting.Equals("SslOnConnect", StringComparison.OrdinalIgnoreCase) || setting.Equals("Ssl", StringComparison.OrdinalIgnoreCase))
                return SecureSocketOptions.SslOnConnect;
            if (setting.Equals("StartTls", StringComparison.OrdinalIgnoreCase))
                return SecureSocketOptions.StartTls;
            if (setting.Equals("None", StringComparison.OrdinalIgnoreCase))
                return SecureSocketOptions.None;

            if (smtp.GetValue("Port", 587) == 465)
                return SecureSocketOptions.SslOnConnect;
            return smtp.GetValue("EnableSsl", true) ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.None;
        }

        /// <summary>Gönderim hatasını yöneticinin anlayacağı Türkçe açıklamaya çevirir (parola içermez)</summary>
        public static string Describe(Exception ex) => ex switch
        {
            AuthenticationException => $"Kullanıcı adı veya parola reddedildi. Email__Smtp__User tam e-posta adresi olmalı; parolayı kontrol edin. ({ex.Message})",
            SslHandshakeException => $"SSL bağlantısı kurulamadı. Port ve Email__Smtp__Security ayarını kontrol edin (465 → SslOnConnect, 587 → StartTls). ({ex.Message})",
            NotSupportedException when ex.Message.Contains("STARTTLS", StringComparison.OrdinalIgnoreCase) =>
                "Sunucu bu portta STARTTLS desteklemiyor. Port 465 ve Email__Smtp__Security=SslOnConnect deneyin.",
            SmtpCommandException cmd => $"Posta sunucusu isteği reddetti ({(int)cmd.StatusCode}): {cmd.Message}",
            SmtpProtocolException => $"Posta sunucusuyla iletişim hatası; port veya güvenlik ayarı yanlış olabilir. ({ex.Message})",
            SocketException or TimeoutException or OperationCanceledException =>
                $"Posta sunucusuna bağlanılamadı veya yanıt alınamadı (adres, port, güvenlik duvarı ya da Email__Smtp__Security ayarı). ({ex.Message})",
            _ => ex.GetBaseException().Message,
        };

        public void SendInBackground(BugReport report, string adminUrl)
        {
            if (!IsConfigured)
                return;
            _ = Task.Run(async () =>
            {
                try
                {
                    await SendReportAsync(report, adminUrl);
                    _logger.LogInformation("Hata bildirimi #{Id} e-postayla iletildi", report.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Hata bildirimi #{Id} e-postası gönderilemedi: {Reason}", report.Id, Describe(ex));
                }
            });
        }

        public Task SendReportAsync(BugReport report, string adminUrl)
        {
            MimePart? attachment = null;
            if (report.Screenshot != null)
            {
                var type = report.ScreenshotContentType ?? "image/jpeg";
                attachment = new MimePart(type)
                {
                    Content = new MimeContent(new MemoryStream(report.Screenshot)),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = $"hata-{report.Id}.{(type == "image/png" ? "png" : "jpg")}",
                };
            }
            return SendAsync($"[Sınav Takvimi] Hata bildirimi #{report.Id}: {Shorten(report.Description, 60)}",
                BuildBody(report, adminUrl), attachment);
        }

        /// <summary>Yönetim ekranındaki "Test e-postası gönder" için; hata olursa fırlatır</summary>
        public Task SendTestAsync(string requestedBy) =>
            SendAsync("[Sınav Takvimi] Test e-postası",
                $"Bu bir deneme e-postasıdır. Hata bildirimleri bu adrese gelecek.\n\nGönderen: {requestedBy}\nZaman: {DateTime.Now:dd.MM.yyyy HH:mm}",
                null);

        private async Task SendAsync(string subject, string body, MimePart? attachment)
        {
            var smtp = _configuration.GetSection("Email:Smtp");
            var host = smtp["Host"]!;
            var port = smtp.GetValue("Port", 587);
            var from = smtp["From"] ?? smtp["User"] ?? throw new InvalidOperationException("Email__Smtp__From ayarı yok.");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Sınav Takvimi", from));
            foreach (var to in Recipients())
                message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            var builder = new BodyBuilder { TextBody = body };
            if (attachment != null)
                builder.Attachments.Add(attachment);
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient { Timeout = 30_000 };
            // Yalnızca yerel testler için (kendinden imzalı sertifika); üretimde kapalı kalmalı
            if (smtp.GetValue("AllowInvalidCertificate", false))
                client.ServerCertificateValidationCallback = (_, _, _, _) => true;

            await client.ConnectAsync(host, port, ResolveSecurity(smtp));
            var user = smtp["User"];
            if (!string.IsNullOrEmpty(user))
            {
                // Sunucu bu bağlantıda giriş sunmuyorsa çoğunlukla şifreli bağlantı bekliyordur
                if (!client.Capabilities.HasFlag(SmtpCapabilities.Authentication))
                    throw new InvalidOperationException(
                        $"Posta sunucusu {port} portunda {(client.IsSecure ? "" : "şifresiz bağlantıda ")}kullanıcı girişi sunmuyor. " +
                        "Port 465 ve Email__Smtp__Security=SslOnConnect deneyin.");
                try
                {
                    await client.AuthenticateAsync(Encoding.UTF8, user, smtp["Password"] ?? "");
                }
                catch (IOException ex)
                {
                    // Bazı sunucular hatalı girişte yanıt vermeden bağlantıyı kapatır
                    throw new AuthenticationException("Sunucu giriş sırasında bağlantıyı kapattı.", ex);
                }
            }
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
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
