using System;

namespace Core.Utilities.Reporting
{
    /// <summary>
    /// Beklenmeyen hataları kalıcı bir yere (hata bildirimleri) kaydeder. Uygulama katmanı uygular;
    /// ExceptionMiddleware ve ExceptionLogger çağırır. Uygulaması asla istisna fırlatmamalıdır.
    /// </summary>
    public interface IUnhandledExceptionReporter
    {
        /// <param name="exception">Yakalanan istisna</param>
        /// <param name="context">Kullanıcıya gösterilen mesaj gibi ek bağlam (varsa)</param>
        void Report(Exception exception, string? context = null);
    }
}
