using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Security.Hashing
{
    /// <summary>
    /// Parola hash'leme: PBKDF2-HMACSHA512, 16 bayt rastgele salt, 64 bayt çıktı.
    ///
    /// Geriye uyumluluk: Eski kayıtlar tek turluk HMACSHA512 ile hash'lenmiş ve salt olarak
    /// 128 baytlık HMAC anahtarı saklanmıştı. Salt uzunluğu formatı ayırt eder:
    ///   - 16 bayt  → PBKDF2 (yeni format)
    ///   - 128 bayt → eski HMACSHA512
    /// Eski formattaki kullanıcı başarılı giriş yaptığında <see cref="NeedsRehash"/> true döner ve
    /// parola yeni formatta yeniden hash'lenir. Veritabanı şemasında değişiklik gerekmez.
    /// </summary>
    public class HashingHelper
    {
        private const int SaltSize = 16;
        private const int HashSize = 64;
        private const int Iterations = 210_000; // OWASP 2023 önerisi (PBKDF2-HMAC-SHA512)

        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            passwordSalt = RandomNumberGenerator.GetBytes(SaltSize);
            passwordHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password), passwordSalt, Iterations, HashAlgorithmName.SHA512, HashSize);
        }

        public static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            if (string.IsNullOrEmpty(password) || passwordHash == null || passwordSalt == null)
                return false;

            byte[] computedHash;
            if (IsLegacyFormat(passwordSalt))
            {
                using var hmac = new HMACSHA512(passwordSalt);
                computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
            else
            {
                computedHash = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(password), passwordSalt, Iterations, HashAlgorithmName.SHA512, passwordHash.Length);
            }

            // Sabit zamanlı karşılaştırma (timing saldırılarına karşı)
            return CryptographicOperations.FixedTimeEquals(computedHash, passwordHash);
        }

        /// <summary>
        /// Hash eski (tek turluk HMACSHA512) formattaysa true döner; giriş sonrası yeniden hash'lenmelidir.
        /// </summary>
        public static bool NeedsRehash(byte[] passwordSalt)
        {
            return passwordSalt == null || IsLegacyFormat(passwordSalt);
        }

        private static bool IsLegacyFormat(byte[] passwordSalt) => passwordSalt.Length != SaltSize;
    }
}
