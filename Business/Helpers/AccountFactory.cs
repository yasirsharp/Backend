using Core.Entities.Concrete;
using Core.Utilities.Security.Hashing;
using Entity.DTOs;
using System.Globalization;
using System.Text;

namespace Business.Helpers
{
    /// <summary>
    /// Yöneticinin açtığı hesaplar için ortak kurallar: geçici parola, ilk girişte parola değiştirme zorunluluğu
    /// ve kullanıcı adı üretimi.
    /// </summary>
    public static class AccountFactory
    {
        /// <summary>
        /// Geçici parolalı yeni bir kullanıcı hazırlar. Parola verilmezse rastgele üretilir.
        /// Her durumda kullanıcı ilk girişte parolasını değiştirmek zorundadır.
        /// </summary>
        public static User NewUser(string firstName, string lastName, string email, string userName,
            out string temporaryPassword, string? password = null)
        {
            temporaryPassword = string.IsNullOrEmpty(password) ? PasswordGenerator.Generate() : password;
            HashingHelper.CreatePasswordHash(temporaryPassword, out var hash, out var salt);

            return new User
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                UserName = userName.Trim(),
                PasswordHash = hash,
                PasswordSalt = salt,
                MustChangePassword = true,
                Status = true,
                CreatedDate = DateTime.Now
            };
        }

        public static CreatedAccountDto ToCreatedAccount(User user, string temporaryPassword) => new()
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            TemporaryPassword = temporaryPassword
        };

        /// <summary>"Ayşe Nur Yılmaz" → ("Ayşe Nur", "Yılmaz"). Tek kelimeyse ad ve soyad aynı olur.</summary>
        public static (string FirstName, string LastName) SplitFullName(string fullName)
        {
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return ("", "");
            if (parts.Length == 1)
                return (parts[0], parts[0]);
            return (string.Join(" ", parts.Take(parts.Length - 1)), parts[^1]);
        }

        /// <summary>
        /// Kullanıcı adlarında Türkçe karakter ve büyük harf kullanılmaz (ör. "Çağrı" → "cagri").
        /// </summary>
        public static string ToAsciiLower(string value)
        {
            var map = new Dictionary<char, char>
            {
                ['ç'] = 'c', ['Ç'] = 'c', ['ğ'] = 'g', ['Ğ'] = 'g', ['ı'] = 'i', ['İ'] = 'i',
                ['ö'] = 'o', ['Ö'] = 'o', ['ş'] = 's', ['Ş'] = 's', ['ü'] = 'u', ['Ü'] = 'u'
            };
            var sb = new StringBuilder(value.Length);
            foreach (var ch in value.Trim())
            {
                var c = map.TryGetValue(ch, out var m) ? m : char.ToLower(ch, CultureInfo.InvariantCulture);
                if (char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-')
                    sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Personel kullanıcı adı: e-posta varsa @ öncesi, yoksa ad soyadın baş harfleri + 4 rakam.
        /// <paramref name="exists"/> ile çakışma kontrol edilir; e-postadan türeyen ad doluysa sonuna rakam eklenir.
        /// </summary>
        public static string GenerateUserName(string fullName, string? email, Func<string, bool> exists)
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                var baseName = ToAsciiLower(email.Split('@')[0]);
                if (baseName.Length > 0)
                {
                    if (!exists(baseName))
                        return baseName;
                    for (var i = 2; i < 100; i++)
                    {
                        var candidate = $"{baseName}{i}";
                        if (!exists(candidate))
                            return candidate;
                    }
                }
            }

            var initials = ToAsciiLower(string.Concat(
                fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(p => p[0])));
            if (initials.Length == 0)
                initials = "personel";

            for (var attempt = 0; attempt < 50; attempt++)
            {
                var candidate = $"{initials}{Random.Shared.Next(1000, 10000)}";
                if (!exists(candidate))
                    return candidate;
            }
            throw new InvalidOperationException("Benzersiz kullanıcı adı üretilemedi.");
        }
    }
}
