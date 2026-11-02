using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Business.Helpers
{
    /// <summary>Hata bildirimi girişlerinin sunucu tarafı kuralları (istemciye güvenilmez)</summary>
    public static class BugReportRules
    {
        public const int MaxScreenshotBytes = 1_500_000;
        public const int MaxClientLogChars = 20_000;
        public const int MaxDescriptionChars = 2000;

        /// <summary>Boşlukları kırpar, sınırı aşan metni keser</summary>
        public static string Clip(string? value, int max)
        {
            var text = (value ?? "").Trim();
            return text.Length <= max ? text : text[..max];
        }

        /// <summary>Dosyanın imzasına göre JPEG veya PNG ise içerik türünü, değilse null döner</summary>
        public static string? DetectImageType(byte[] data)
        {
            if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
                return "image/jpeg";
            if (data.Length >= 8 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47
                && data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A)
                return "image/png";
            return null;
        }

        private static readonly Regex IdSegment = new(@"^(\d+|[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})$", RegexOptions.Compiled);
        private static readonly Regex Digits = new(@"\d+", RegexOptions.Compiled);
        private static readonly Regex AsyncName = new(@"^<(?<name>[^>]+)>.*$", RegexOptions.Compiled);
        private static readonly string[] AppNamespaces = { "API.", "Business.", "DataAccess.", "Core.", "Entity." };

        /// <summary>"/api/SinavDetay/42?x=1" → "/api/sinavdetay/{id}" (sorgu ve kimlikler atılır)</summary>
        public static string NormalizePath(string? path)
        {
            var clean = (path ?? "").Split('?', '#')[0];
            if (Uri.TryCreate(clean, UriKind.Absolute, out var uri))
                clean = uri.AbsolutePath;
            var segments = clean.Split('/').Select(s => IdSegment.IsMatch(s) ? "{id}" : s.ToLowerInvariant());
            return string.Join('/', segments);
        }

        /// <summary>Sayıları # yapar: aynı hatanın farklı kayıtlarda geçen mesajları eşleşsin</summary>
        public static string NormalizeMessage(string? message) =>
            Clip(Digits.Replace(message ?? "", "#"), 300);

        /// <summary>İstisnanın ilk uygulama kodu çerçevesi ("Business.Concrete.SinavDetayManager.Add")</summary>
        public static string AppFrame(Exception exception)
        {
            foreach (var frame in new StackTrace(exception, false).GetFrames())
            {
                var method = frame.GetMethod();
                var type = method?.DeclaringType;
                if (type == null) continue;
                // async metotlarda derleyicinin ürettiği sınıf: <AddAsync>d__5.MoveNext → asıl sınıf + AddAsync
                var name = method!.Name;
                var match = AsyncName.Match(type.Name);
                if (match.Success && type.DeclaringType != null)
                {
                    name = match.Groups["name"].Value;
                    type = type.DeclaringType;
                }
                var full = type.FullName ?? type.Name;
                if (AppNamespaces.Any(ns => full.StartsWith(ns, StringComparison.Ordinal)))
                    return $"{full}.{name}";
            }
            return "";
        }

        /// <summary>Parçalardan 64 karakterlik parmak izi (SHA-256, onaltılık)</summary>
        public static string Fingerprint(params string?[] parts) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", parts.Select(p => p ?? ""))))).ToLowerInvariant();

        /// <summary>Ekran görüntüsünü doğrular; hata mesajı veya null (geçerli) döner</summary>
        public static string? ValidateScreenshot(byte[]? data)
        {
            if (data == null || data.Length == 0)
                return null;
            if (data.Length > MaxScreenshotBytes)
                return "Ekran görüntüsü çok büyük (en fazla 1,5 MB).";
            if (DetectImageType(data) == null)
                return "Ekran görüntüsü JPEG veya PNG olmalıdır.";
            return null;
        }
    }
}
