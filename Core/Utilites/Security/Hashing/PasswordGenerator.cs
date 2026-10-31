using System.Security.Cryptography;

namespace Core.Utilities.Security.Hashing
{
    public static class PasswordGenerator
    {
        // Karıştırılabilecek karakterler (0/O, 1/l/I) çıkarıldı
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";

        public static string Generate(int length = 12)
        {
            return RandomNumberGenerator.GetString(Alphabet, length);
        }
    }
}
