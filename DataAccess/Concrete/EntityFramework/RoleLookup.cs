using Microsoft.EntityFrameworkCore;

namespace DataAccess.Concrete.EntityFramework
{
    public static class RoleLookup
    {
        /// <summary>
        /// Rol adından ID bulur. Roller migration tohum verisiyle oluşturulur; kodda sabit ID kullanılmaz.
        /// </summary>
        public static async Task<int> GetRoleIdAsync(this DuzceUniversiteContext context, string roleName)
        {
            var id = await context.OperationClaims
                .Where(c => c.Name == roleName)
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync();

            return id ?? throw new InvalidOperationException(
                $"'{roleName}' rolü veritabanında bulunamadı. Veritabanı şeması (db/schema.sql) uygulanmış mı?");
        }
    }
}
