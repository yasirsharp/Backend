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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer(@"Server=172.16.4.4; Database=ysr.sinav_takvimi; User Id=yasir; Password=Yasir0308; Trust Server Certificate=True;");
            optionsBuilder.UseSqlServer(@"Server=YASIR\DUPROJECTS; Database=DuzceUniversite; Trust Server Certificate=True; User Id = sa; Password = 123456Aa");
            //optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=DuzceUniversite;Trusted_Connection=true");
        }
    }
} 