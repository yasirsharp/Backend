using Core.Entities.Concrete;
using Entity.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccess.Concrete.EntityFramework
{
    public class DuzceUniversiteContext : DbContext
    {
        public DbSet<Bolum> Bolum { get; set; }
        public DbSet<Ders> Ders { get; set; }
        public DbSet<AkademikPersonel> AkademikPersonel { get; set; }
        public DbSet<Derslik> Derslik { get; set; }
        public DbSet<DersBolumAkademikPersonel> DersBolumAkademikPersonel { get; set; }
        public DbSet<BolumAkademikPersoneller> BolumAkademikPersoneller { get; set; }
        public DbSet<SinavDetay> SinavDetay { get; set; }
        public DbSet<SinavDerslik> SinavDerslik { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<OperationClaim> OperationClaims { get; set; }
        public DbSet<UserOperationClaim> UserOperationClaims { get; set; }
        public DbSet<DersBolum> DersBolum { get; set; }
        public DbSet<DerslikBolum> DerslikBolum { get; set; }
        
        // Ogrenci System
        public DbSet<Ogrenci> Ogrenci { get; set; }
        
        // Notification System
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<AnnouncementReadStatus> AnnouncementReadStatus { get; set; }
        
        // YasirSharp AI - Assistant System (26 Ekim 2025)
        public DbSet<AssistantInteraction> AssistantInteractions { get; set; }
        public DbSet<UserAssistantPreference> UserAssistantPreferences { get; set; }

        // Akademik Personel Müsaitlik Sistemi
        public DbSet<AkademikPersonelMusaitlik> AkademikPersonelMusaitlik { get; set; }

        // Uygulama içinden gönderilen hata bildirimleri
        public DbSet<BugReport> BugReports { get; set; }

        /// <summary>
        /// Uygulama açılışında "ConnectionStrings:DefaultConnection" ayarından atanır.
        /// Bağlantı bilgisi (özellikle parola) koda veya repoya yazılmamalıdır; User Secrets ya da
        /// ConnectionStrings__DefaultConnection ortam değişkeni kullanılmalıdır.
        /// </summary>
        public static string? ConnectionString { get; set; }

        /// <summary>
        /// Veritabanı türü ("Database:Provider" ayarı). Varsayılan SQL Server; test sunucuları için MySQL/MariaDB.
        /// MySQL migration'ları ayrı DataAccess.MySql projesindedir (EF, bir assembly'de bir context için tek
        /// model snapshot'a izin verir).
        /// </summary>
        public static DatabaseProvider Provider { get; set; } = DatabaseProvider.SqlServer;

        /// <summary>MySQL sürümü (ör. "8.0.36-mysql", "10.11-mariadb"). Boşsa ilk bağlantıda sunucudan okunur.</summary>
        public static string? MySqlServerVersion { get; set; }

        public const string MySqlMigrationsAssembly = "DataAccess.MySql";

        private static ServerVersion? _detectedMySqlVersion;

        private static ServerVersion GetMySqlVersion()
        {
            if (!string.IsNullOrWhiteSpace(MySqlServerVersion))
                return ServerVersion.Parse(MySqlServerVersion);
            // Her context oluşturulduğunda sunucuya sormamak için bir kez algılanır
            return _detectedMySqlVersion ??= ServerVersion.AutoDetect(ConnectionString);
        }

        /// <summary>
        /// Rollerin sabit ID'leri. Tohum verisi (HasData) ile oluşturulur; kod rol adlarını kullanır.
        /// </summary>
        public static readonly (int Id, string Name)[] SeedRoles =
        {
            (1, "super.admin"),
            (2, "akademik.personel"),
            (3, "ogrenci"),
            (4, "bolum.gorevlisi"),
            (5, "gorevli.personel"),
            (6, "Admin"),
        };

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (Database.IsMySql())
            {
                // SQL Server'daki gibi büyük/küçük harfe duyarsız karşılaştırma (e-posta, kullanıcı adı, arama).
                // Türkçe collation kullanılmaz: I → ı dönüşümü e-posta karşılaştırmalarını bozar.
                modelBuilder.HasCharSet("utf8mb4").UseCollation("utf8mb4_unicode_ci");
            }

            // --- Kullanıcılar ve yetkiler ---
            modelBuilder.Entity<User>(e =>
            {
                e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
                e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
                e.Property(x => x.Email).HasMaxLength(256).IsRequired();
                e.Property(x => x.UserName).HasMaxLength(100).IsRequired();
                e.Property(x => x.PasswordHash).HasMaxLength(128).IsRequired();
                e.Property(x => x.PasswordSalt).HasMaxLength(128).IsRequired();
                e.HasIndex(x => x.Email).IsUnique();
                e.HasIndex(x => x.UserName).IsUnique();
            });

            modelBuilder.Entity<OperationClaim>(e =>
            {
                e.Property(x => x.Name).HasMaxLength(100).IsRequired();
                e.HasIndex(x => x.Name).IsUnique();
                var seedDate = new DateTime(2026, 1, 1);
                e.HasData(SeedRoles.Select(r => new OperationClaim { Id = r.Id, Name = r.Name, CreatedDate = seedDate, Status = true }));
            });

            modelBuilder.Entity<UserOperationClaim>(e => e.HasIndex(x => new { x.UserId, x.OperationClaimId }).IsUnique());

            // --- Tanımlar ---
            modelBuilder.Entity<Bolum>(e =>
            {
                e.Property(x => x.Ad).HasMaxLength(200).IsRequired();
                e.HasIndex(x => x.Ad).IsUnique();
            });

            modelBuilder.Entity<Ders>(e =>
            {
                e.Property(x => x.Ad).HasMaxLength(200).IsRequired();
                e.Property(x => x.Kod).HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.Kod).IsUnique();
            });

            modelBuilder.Entity<Derslik>(e =>
            {
                e.Property(x => x.Ad).HasMaxLength(100).IsRequired();
                e.HasIndex(x => x.Ad).IsUnique();
            });

            modelBuilder.Entity<AkademikPersonel>(e =>
            {
                e.Property(x => x.Ad).HasMaxLength(200).IsRequired();
                e.Property(x => x.Unvan).HasMaxLength(100).IsRequired();
                e.HasIndex(x => x.UserId).IsUnique();
            });

            modelBuilder.Entity<Ogrenci>(e =>
            {
                e.Property(x => x.OgrenciNo).HasMaxLength(20).IsRequired();
                e.Property(x => x.Ad).HasMaxLength(100).IsRequired();
                e.Property(x => x.Soyad).HasMaxLength(100).IsRequired();
                e.HasIndex(x => x.OgrenciNo).IsUnique();
                e.HasIndex(x => x.UserId).IsUnique();
                e.HasIndex(x => x.BolumId);
            });

            // --- İlişki tabloları (aynı eşleşme iki kez eklenemez) ---
            modelBuilder.Entity<DersBolum>(e => e.HasIndex(x => new { x.DersId, x.BolumId }).IsUnique());
            modelBuilder.Entity<DerslikBolum>(e => e.HasIndex(x => new { x.DerslikId, x.BolumId }).IsUnique());
            modelBuilder.Entity<BolumAkademikPersoneller>(e => e.HasIndex(x => new { x.BolumId, x.AkademikPersonelId }).IsUnique());
            modelBuilder.Entity<DersBolumAkademikPersonel>(e =>
            {
                e.HasIndex(x => new { x.DersId, x.BolumId, x.AkademikPersonelId }).IsUnique();
                e.HasIndex(x => x.AkademikPersonelId);
            });

            // --- Sınavlar ---
            modelBuilder.Entity<SinavDetay>(e =>
            {
                e.HasIndex(x => x.SinavTarihi);
                e.HasIndex(x => x.DersBolumAkademikPersonelId);
            });
            modelBuilder.Entity<SinavDerslik>(e =>
            {
                e.HasIndex(x => x.SinavDetayId);
                e.HasIndex(x => x.DerslikId);
                e.HasIndex(x => x.GozetmenId);
            });

            modelBuilder.Entity<AkademikPersonelMusaitlik>(e => e.HasIndex(x => new { x.AkademikPersonelId, x.BaslangicTarihi }));

            // --- Duyuru ve bildirimler ---
            modelBuilder.Entity<Notification>(e =>
            {
                e.Property(x => x.Title).HasMaxLength(200).IsRequired();
                e.Property(x => x.Message).HasMaxLength(2000).IsRequired();
                e.Property(x => x.Type).HasMaxLength(20).IsRequired();
                e.Property(x => x.ActionUrl).HasMaxLength(500);
                e.Property(x => x.RelatedEntityType).HasMaxLength(50);
                e.HasIndex(x => new { x.UserId, x.IsRead });
            });
            modelBuilder.Entity<Announcement>(e =>
            {
                e.Property(x => x.Title).HasMaxLength(200).IsRequired();
                e.Property(x => x.Type).HasMaxLength(30).IsRequired();
                e.Property(x => x.TargetAudience).HasMaxLength(50).IsRequired();
            });
            modelBuilder.Entity<AnnouncementReadStatus>(e => e.HasIndex(x => new { x.AnnouncementId, x.UserId }).IsUnique());

            modelBuilder.Entity<BugReport>(e =>
            {
                e.Property(x => x.UserName).HasMaxLength(200).IsRequired();
                e.Property(x => x.Roles).HasMaxLength(300).IsRequired();
                e.Property(x => x.Description).HasMaxLength(2000).IsRequired();
                e.Property(x => x.PageUrl).HasMaxLength(500).IsRequired();
                e.Property(x => x.AppVersion).HasMaxLength(100).IsRequired();
                e.Property(x => x.UserAgent).HasMaxLength(500).IsRequired();
                e.Property(x => x.Viewport).HasMaxLength(50).IsRequired();
                e.Property(x => x.ScreenshotContentType).HasMaxLength(50);
                e.Property(x => x.State).HasMaxLength(20).IsRequired();
                e.Property(x => x.AdminNote).HasMaxLength(2000);
                e.HasIndex(x => x.State);
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException("Veritabanı bağlantı dizesi yapılandırılmamış (ConnectionStrings:DefaultConnection).");

            if (Provider == DatabaseProvider.MySql)
                optionsBuilder.UseMySql(ConnectionString, GetMySqlVersion(),
                    mysql => mysql.MigrationsAssembly(MySqlMigrationsAssembly));
            else
                optionsBuilder.UseSqlServer(ConnectionString);
        }
    }
}

namespace DataAccess.Concrete.EntityFramework
{
    public enum DatabaseProvider
    {
        SqlServer,
        MySql
    }
}
