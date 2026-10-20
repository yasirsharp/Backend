using Core.Entities.Concrete;
using Core.Utilities.Security.Hashing;
using DataAccess.Concrete.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace API.Startup
{
    /// <summary>
    /// Açılışta veritabanını hazırlar:
    /// - Database:MigrateOnStartup=true ise bekleyen migration'ları uygular (varsayılan kapalı; şema db/schema.sql ile kurulur).
    /// - Seed:AdminEmail ve Seed:AdminPassword tanımlıysa ve hiç super.admin yoksa ilk yöneticiyi oluşturur.
    ///   Bu hesap ilk girişte parolasını değiştirmek zorundadır. Parola loglanmaz.
    /// </summary>
    public static class DatabaseInitializer
    {
        public const string SuperAdminRole = "super.admin";

        public static async Task InitializeAsync(IConfiguration configuration, ILogger logger)
        {
            await using var context = new DuzceUniversiteContext();

            if (configuration.GetValue<bool>("Database:MigrateOnStartup"))
            {
                logger.LogInformation("Bekleyen veritabanı migration'ları uygulanıyor...");
                await context.Database.MigrateAsync();
            }

            await SeedAdminAsync(context, configuration, logger);
        }

        private static async Task SeedAdminAsync(DuzceUniversiteContext context, IConfiguration configuration, ILogger logger)
        {
            var email = configuration["Seed:AdminEmail"]?.Trim().ToLowerInvariant();
            var password = configuration["Seed:AdminPassword"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
                return;

            var roleId = await context.GetRoleIdAsync(SuperAdminRole);
            var hasSuperAdmin = await (from uoc in context.UserOperationClaims
                                       join u in context.Users on uoc.UserId equals u.Id
                                       where uoc.OperationClaimId == roleId && u.Status
                                       select uoc.Id).AnyAsync();
            if (hasSuperAdmin)
                return;

            if (password.Length < 8)
                throw new InvalidOperationException("Seed:AdminPassword en az 8 karakter olmalıdır.");

            var userName = configuration["Seed:AdminUserName"]?.Trim();
            if (string.IsNullOrWhiteSpace(userName))
                userName = email.Split('@')[0];

            if (await context.Users.AnyAsync(u => u.Email == email || u.UserName == userName))
                throw new InvalidOperationException(
                    $"İlk yönetici oluşturulamadı: '{email}' e-postası veya '{userName}' kullanıcı adı başka bir hesapta kullanılıyor. " +
                    "Seed:AdminEmail / Seed:AdminUserName değerlerini değiştirin.");

            HashingHelper.CreatePasswordHash(password, out var hash, out var salt);
            var user = new User
            {
                FirstName = configuration["Seed:AdminFirstName"] ?? "Sistem",
                LastName = configuration["Seed:AdminLastName"] ?? "Yöneticisi",
                Email = email,
                UserName = userName,
                PasswordHash = hash,
                PasswordSalt = salt,
                MustChangePassword = true,
                Status = true,
                CreatedDate = DateTime.Now
            };

            await using var transaction = await context.Database.BeginTransactionAsync();
            context.Users.Add(user);
            await context.SaveChangesAsync();
            context.UserOperationClaims.Add(new UserOperationClaim
            {
                UserId = user.Id,
                OperationClaimId = roleId,
                CreatedDate = DateTime.Now,
                Status = true
            });
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            logger.LogWarning(
                "İlk yönetici hesabı oluşturuldu: {UserName} ({Email}). İlk girişte parola değiştirilecek. " +
                "Seed:AdminPassword ayarını artık kaldırabilirsiniz.", userName, email);
        }
    }
}
