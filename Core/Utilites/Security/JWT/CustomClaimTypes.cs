namespace Core.Utilities.Security.JWT
{
    public static class CustomClaimTypes
    {
        /// <summary>
        /// Kullanıcı geçici parolayla giriş yaptı; parolasını değiştirene kadar diğer uçlar kapalıdır.
        /// Frontend bu claim'i görünce parola değiştirme sayfasına yönlendirir.
        /// </summary>
        public const string PasswordChangeRequired = "pwd_change";
    }
}
