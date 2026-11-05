using System.Collections.Concurrent;

namespace Business.Helpers
{
    /// <summary>
    /// Hesap bazında kaba kuvvet koruması: aynı e-posta/kullanıcı adı için 15 dakikada 5 hatalı parola
    /// denemesinden sonra o hesaba giriş 15 dakika kapatılır.
    /// IP bazlı hız sınırı tek başına yetmez: kampüste çok sayıda kullanıcı aynı NAT/proxy IP'sinden gelir,
    /// sınır düşük tutulursa herkes engellenir, yüksek tutulursa tek hesaba yönelik denemeleri durdurmaz.
    /// Sayaçlar bellektedir (tek sunucu); uygulama yeniden başlayınca sıfırlanır.
    /// </summary>
    public static class LoginThrottle
    {
        public const int MaxFailures = 5;
        public static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

        private static readonly ConcurrentDictionary<string, (int Count, DateTime FirstFailureUtc)> Failures = new();

        private static string Key(string identifier) => identifier.Trim().ToLowerInvariant();

        public static bool IsLocked(string identifier, DateTime? nowUtc = null)
        {
            var now = nowUtc ?? DateTime.UtcNow;
            if (!Failures.TryGetValue(Key(identifier), out var entry))
                return false;
            if (now - entry.FirstFailureUtc > Window)
            {
                Failures.TryRemove(Key(identifier), out _);
                return false;
            }
            return entry.Count >= MaxFailures;
        }

        public static void RegisterFailure(string identifier, DateTime? nowUtc = null)
        {
            var now = nowUtc ?? DateTime.UtcNow;
            Failures.AddOrUpdate(Key(identifier),
                _ => (1, now),
                (_, e) => now - e.FirstFailureUtc > Window ? (1, now) : (e.Count + 1, e.FirstFailureUtc));

            // Rastgele kullanıcı adlarıyla sözlüğün şişmesini önle
            if (Failures.Count > 10_000)
                foreach (var old in Failures.Where(kv => now - kv.Value.FirstFailureUtc > Window).Select(kv => kv.Key).ToList())
                    Failures.TryRemove(old, out _);
        }

        public static void Reset(string identifier) => Failures.TryRemove(Key(identifier), out _);
    }
}
