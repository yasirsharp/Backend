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
