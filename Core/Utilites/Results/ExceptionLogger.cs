using Core.Utilities.IoC;
using Core.Utilities.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.Utilities.Results
{
    /// <summary>
    /// Manager'lardaki catch bloklarında kullanılır: istisnanın ayrıntısını (SQL hatası, stack trace vb.)
    /// sunucu loguna yazar ve istemciye sadece kullanıcı dostu mesajı döndürür.
    /// Önceden ex.Message doğrudan API yanıtına ekleniyordu ve veritabanı yapısı dışarı sızıyordu.
    /// </summary>
    public static class ExceptionLogger
    {
        private static ILogger? _logger;

        /// <summary>Açılışta atanır: yakalanan hatalar hata bildirimlerine de kaydedilir</summary>
        public static IUnhandledExceptionReporter? Reporter { get; set; }

        private static ILogger? Logger =>
            _logger ??= ServiceTool.ServiceProvider?.GetService<ILoggerFactory>()?.CreateLogger("Business");

        public static string Log(Exception exception, string userMessage)
        {
            Logger?.LogError(exception, "{UserMessage}", userMessage);
            try
            {
                Reporter?.Report(exception, userMessage);
            }
            catch
            {
                // Kayıt başarısız olsa da asıl işlemin hata mesajı döner
            }
            return userMessage;
        }
    }
}
