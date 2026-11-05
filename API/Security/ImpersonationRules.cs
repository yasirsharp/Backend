namespace API.Security
{
    /// <summary>"Bu kullanıcı olarak gir" kuralları (saf, test edilebilir)</summary>
    public static class ImpersonationRules
    {
        /// <summary>Hedefe geçilemiyorsa nedeni, geçilebiliyorsa null</summary>
        public static string? Check(bool actorIsSuperAdmin, int actorId, int targetId, bool targetActive, IEnumerable<string> targetRoles)
        {
            if (!actorIsSuperAdmin)
                return "Kullanıcı olarak girme yalnızca süper yöneticiye açıktır.";
            if (actorId == targetId)
                return "Zaten kendi hesabınızdasınız.";
            if (!targetActive)
                return "Pasif bir hesaba geçilemez.";
            if (targetRoles.Contains(Business.Constants.Roles.SuperAdmin))
                return "Başka bir süper yöneticinin hesabına geçilemez.";
            return null;
        }
    }
}
