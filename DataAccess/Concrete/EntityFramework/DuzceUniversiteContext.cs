using Core.Entities.Concrete;
using Entity.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Uygulama açılışında "ConnectionStrings:DefaultConnection" ayarından atanır.
        /// Bağlantı bilgisi (özellikle parola) koda veya repoya yazılmamalıdır; User Secrets ya da
        /// ConnectionStrings__DefaultConnection ortam değişkeni kullanılmalıdır.
        /// </summary>
        public static string? ConnectionString { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException("Veritabanı bağlantı dizesi yapılandırılmamış (ConnectionStrings:DefaultConnection).");

            optionsBuilder.UseSqlServer(ConnectionString);
        }
    }
} 