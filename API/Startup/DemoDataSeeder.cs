using Business.Constants;
using Business.Helpers;
using Core.Entities.Concrete;
using Core.Utilities.Security.Hashing;
using DataAccess.Concrete.EntityFramework;
using Entity.Concrete;
using Microsoft.EntityFrameworkCore;

namespace API.Startup
{
    /// <summary>
    /// Sunum ve deneme için örnek veri: Seed:DemoData=true ve Seed:DemoPassword (en az 8 karakter) verilirse açılışta
    /// bir kez kurulur. Mevcut kayıtlara dokunmaz; aynı adlı bölüm, derslik ve dersleri yeniden kullanır.
    /// Bütün demo hesapları aynı parolayla (Seed:DemoPassword) girer ve parola değiştirmek zorunda değildir.
    /// "demo.yonetici" hesabı varsa demo verisi kurulmuş sayılır ve tekrar kurulmaz.
    /// Adlar ve e-postalar uydurmadır (@demo.duzce.edu.tr).
    /// </summary>
    public static class DemoDataSeeder
    {
        public const string MarkerUserName = "demo.yonetici";
        public const string EmailDomain = "demo.duzce.edu.tr";

        private record BolumDef(string Kisa, string Ad);
        private record DerslikDef(string Ad, int Kapasite, string[] Bolumler, bool Aktif = true);
        private record DersDef(string Kod, string Ad, string[] Bolumler);
        private record PersonelDef(string Unvan, string Ad, string Bolum);
        private record AtamaDef(string Kod, string Bolum, string Personel);

        private static readonly BolumDef[] Bolumler =
        {
            new("BLM", "Bilgisayar Mühendisliği"),
            new("EEM", "Elektrik-Elektronik Mühendisliği"),
            new("INS", "İnşaat Mühendisliği"),
            new("MAK", "Makine Mühendisliği"),
        };

        private static readonly DerslikDef[] Derslikler =
        {
            new("MF-A101", 60, new[] { "BLM" }),
            new("MF-A102", 50, new[] { "BLM" }),
            new("BLM-LAB1", 40, new[] { "BLM" }),
            new("MF-A201", 60, new[] { "EEM" }),
            new("MF-A202", 45, new[] { "EEM" }),
            new("MF-B101", 70, new[] { "INS" }),
            new("MF-B102", 50, new[] { "INS" }),
            new("MF-B201", 60, new[] { "MAK" }),
            new("MF-B202", 45, new[] { "MAK", "INS" }),
            new("AMFI-1", 150, Array.Empty<string>()),
            new("AMFI-2", 120, Array.Empty<string>()),
            new("MF-Z01", 40, Array.Empty<string>(), Aktif: false),
        };

        private static readonly string[] Hepsi = { "BLM", "EEM", "INS", "MAK" };

        private static readonly DersDef[] Dersler =
        {
            new("BLM105", "Programlamaya Giriş", new[] { "BLM" }),
            new("BLM201", "Veri Yapıları", new[] { "BLM" }),
            new("BLM203", "Bilgisayar Organizasyonu", new[] { "BLM" }),
            new("BLM301", "Veritabanı Yönetim Sistemleri", new[] { "BLM" }),
            new("BLM305", "İşletim Sistemleri", new[] { "BLM" }),
            new("BLM401", "Yazılım Mühendisliği", new[] { "BLM" }),
            new("BLM403", "Bilgisayar Ağları", new[] { "BLM" }),
            new("EEM101", "Elektrik Mühendisliğine Giriş", new[] { "EEM" }),
            new("EEM203", "Devre Analizi I", new[] { "EEM" }),
            new("EEM205", "Elektronik I", new[] { "EEM" }),
            new("EEM301", "Elektromanyetik Alan Teorisi", new[] { "EEM" }),
            new("EEM305", "Sinyaller ve Sistemler", new[] { "EEM" }),
            new("EEM401", "Güç Sistemleri", new[] { "EEM" }),
            new("INS101", "Statik", new[] { "INS" }),
            new("INS201", "Mukavemet", new[] { "INS" }),
            new("INS205", "Yapı Malzemesi", new[] { "INS" }),
            new("INS301", "Zemin Mekaniği", new[] { "INS" }),
            new("INS303", "Betonarme I", new[] { "INS" }),
            new("MAK101", "Teknik Resim", new[] { "MAK" }),
            new("MAK201", "Termodinamik I", new[] { "MAK" }),
            new("MAK203", "Dinamik", new[] { "MAK" }),
            new("MAK301", "Akışkanlar Mekaniği", new[] { "MAK" }),
            new("MAK305", "Makine Elemanları I", new[] { "MAK" }),
            new("MUH201", "Mühendislik Ekonomisi", new[] { "INS", "MAK" }),
            // Ortak dersler (bölümsüz)
            new("MAT101", "Matematik I", Array.Empty<string>()),
            new("FIZ101", "Fizik I", Array.Empty<string>()),
            new("TDL101", "Türk Dili I", Array.Empty<string>()),
            new("ATA101", "Atatürk İlkeleri ve İnkılap Tarihi I", Array.Empty<string>()),
            new("ING101", "İngilizce I", Array.Empty<string>()),
        };

        private static readonly PersonelDef[] Personeller =
        {
            new("Prof. Dr.", "Ahmet Yılmaz", "BLM"),
            new("Doç. Dr.", "Elif Kaya", "BLM"),
            new("Dr. Öğr. Üyesi", "Mehmet Demir", "BLM"),
            new("Dr. Öğr. Üyesi", "Zeynep Arslan", "BLM"),
            new("Öğr. Gör.", "Burak Çelik", "BLM"),
            new("Arş. Gör.", "Selin Aydın", "BLM"),
            new("Prof. Dr.", "Hasan Öztürk", "EEM"),
            new("Doç. Dr.", "Ayşe Şahin", "EEM"),
            new("Dr. Öğr. Üyesi", "Murat Koç", "EEM"),
            new("Öğr. Gör.", "Deniz Kurt", "EEM"),
            new("Arş. Gör.", "Emre Polat", "EEM"),
            new("Prof. Dr.", "Mustafa Aksoy", "INS"),
            new("Doç. Dr.", "Fatma Özdemir", "INS"),
            new("Dr. Öğr. Üyesi", "Can Erdoğan", "INS"),
            new("Arş. Gör.", "Gizem Yıldız", "INS"),
            new("Prof. Dr.", "Ali Kılıç", "MAK"),
            new("Doç. Dr.", "Merve Doğan", "MAK"),
            new("Dr. Öğr. Üyesi", "Serkan Aslan", "MAK"),
            new("Arş. Gör.", "Ece Güneş", "MAK"),
            new("Dr. Öğr. Üyesi", "Okan Yavuz", "ORTAK"),
            new("Dr. Öğr. Üyesi", "Pınar Karaca", "ORTAK"),
            new("Öğr. Gör.", "Hülya Tekin", "ORTAK"),
            new("Öğr. Gör.", "Kemal Bulut", "ORTAK"),
            new("Öğr. Gör.", "Derya Uçar", "ORTAK"),
        };

        /// <summary>Bölüm görevlileri (bölüm → personel)</summary>
        private static readonly (string Bolum, string Personel)[] Gorevliler =
        {
            ("BLM", "Elif Kaya"), ("EEM", "Ayşe Şahin"), ("INS", "Fatma Özdemir"), ("MAK", "Merve Doğan"),
        };

        /// <summary>BLM403 ve EEM401 bilerek atanmadı: "öğretim elemanı atanmamış ders" uyarısı görünsün</summary>
        private static readonly AtamaDef[] Atamalar = BuildAtamalar();

        private static AtamaDef[] BuildAtamalar()
        {
            var list = new List<AtamaDef>
            {
                new("BLM105", "BLM", "Burak Çelik"), new("BLM201", "BLM", "Ahmet Yılmaz"), new("BLM203", "BLM", "Zeynep Arslan"),
                new("BLM301", "BLM", "Mehmet Demir"), new("BLM305", "BLM", "Elif Kaya"), new("BLM401", "BLM", "Ahmet Yılmaz"),
                new("EEM101", "EEM", "Deniz Kurt"), new("EEM203", "EEM", "Murat Koç"), new("EEM205", "EEM", "Ayşe Şahin"),
                new("EEM301", "EEM", "Hasan Öztürk"), new("EEM305", "EEM", "Murat Koç"),
                new("INS101", "INS", "Can Erdoğan"), new("INS201", "INS", "Mustafa Aksoy"), new("INS205", "INS", "Fatma Özdemir"),
                new("INS301", "INS", "Mustafa Aksoy"), new("INS303", "INS", "Fatma Özdemir"),
                new("MAK101", "MAK", "Serkan Aslan"), new("MAK201", "MAK", "Ali Kılıç"), new("MAK203", "MAK", "Merve Doğan"),
                new("MAK301", "MAK", "Ali Kılıç"), new("MAK305", "MAK", "Serkan Aslan"),
                new("MUH201", "INS", "Can Erdoğan"), new("MUH201", "MAK", "Serkan Aslan"),
            };
            foreach (var b in Hepsi)
            {
                list.Add(new("MAT101", b, "Okan Yavuz"));
                list.Add(new("FIZ101", b, "Pınar Karaca"));
                list.Add(new("TDL101", b, "Hülya Tekin"));
                list.Add(new("ATA101", b, "Kemal Bulut"));
                list.Add(new("ING101", b, "Derya Uçar"));
            }
            return list.ToArray();
        }

        private static readonly string[] ErkekAdlari = { "Ahmet", "Mehmet", "Mustafa", "Emre", "Burak", "Kerem", "Yusuf", "Ömer", "Berk", "Can", "Eren", "Furkan", "Onur", "Tolga", "Umut", "Batuhan", "Arda", "Kaan", "Oğuz", "Serhat" };
        private static readonly string[] KadinAdlari = { "Ayşe", "Elif", "Zeynep", "Merve", "Büşra", "Esra", "Gamze", "İrem", "Ceren", "Damla", "Sena", "Ecem", "Nisa", "Dilara", "Melis", "Selin", "Buse", "Yağmur", "Özge", "Tuğçe" };
        private static readonly string[] Soyadlari = { "Yılmaz", "Kaya", "Demir", "Şahin", "Çelik", "Yıldız", "Yıldırım", "Öztürk", "Aydın", "Özdemir", "Arslan", "Doğan", "Kılıç", "Aslan", "Çetin", "Kara", "Koç", "Kurt", "Özkan", "Şimşek", "Polat", "Erdem", "Güneş", "Aksoy", "Bulut", "Tekin", "Uçar", "Ateş", "Karaca", "Bozkurt" };

        private static readonly (TimeOnly Bas, TimeOnly Bit)[] Oturumlar =
        {
            (new(9, 0), new(10, 30)), (new(11, 0), new(12, 30)), (new(13, 30), new(15, 0)), (new(15, 30), new(17, 0)),
        };

        public static async Task SeedAsync(IConfiguration configuration, ILogger logger)
        {
            if (!configuration.GetValue<bool>("Seed:DemoData"))
                return;
            var password = configuration["Seed:DemoPassword"];
            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                logger.LogWarning("Seed:DemoData açık ama Seed:DemoPassword (en az 8 karakter) yok; demo verisi kurulmadı.");
                return;
            }

            await using var context = new DuzceUniversiteContext();
            if (await context.Users.AnyAsync(u => u.UserName == MarkerUserName))
            {
                logger.LogInformation("Demo verisi zaten kurulu ({User} var); atlandı. Seed__DemoData satırını kaldırabilirsiniz.", MarkerUserName);
                return;
            }

            logger.LogWarning("Demo verisi kuruluyor...");
            var started = DateTime.Now;
            HashingHelper.CreatePasswordHash(password, out var hash, out var salt);
            var seeder = new Run(context, hash, salt);

            await using (var transaction = await context.Database.BeginTransactionAsync())
            {
                await seeder.SeedAllAsync();
                await transaction.CommitAsync();
            }

            // Sınav bildirimleri (hoca ve gözetmenlere), uygulamanın kendi bildirim metinleriyle
            var sinavDal = new EfSinavDetayDal();
            var bildirimci = new SinavBildirimci(new EfNotificationDal());
            foreach (var id in seeder.SinavIds)
                bildirimci.Eklendi(sinavDal.GetBildirimBilgisi(id));

            logger.LogWarning(
                "Demo verisi kuruldu ({Seconds:0} sn): {Bolum} bölüm, {Derslik} derslik, {Ders} ders, {Personel} personel, {Ogrenci} öğrenci, " +
                "{Sinav} sınav. Demo hesapları Seed:DemoPassword parolasıyla girer (ör. {Marker}). Seed__DemoData satırını artık kaldırabilirsiniz.",
                (DateTime.Now - started).TotalSeconds, Bolumler.Length, Derslikler.Length, Dersler.Length, Personeller.Length,
                seeder.OgrenciSayisi, seeder.SinavIds.Count, MarkerUserName);
        }

        /// <summary>Tek bir kurulum çalıştırmasının durumu</summary>
        private sealed class Run
        {
            private readonly DuzceUniversiteContext _c;
            private readonly byte[] _hash;
            private readonly byte[] _salt;
            private readonly DateTime _now = DateTime.Now;
            private readonly Random _random = new(2026);

            private readonly Dictionary<string, int> _bolum = new();
            private readonly Dictionary<string, (int Id, int Kapasite, string[] Bolumler)> _derslik = new();
            private readonly Dictionary<string, int> _ders = new();
            private readonly Dictionary<string, (int Id, int UserId, string Unvan, string Bolum)> _personel = new();
            private readonly List<AkademikPersonelMusaitlik> _musaitlik = new();
            private int _adminUserId;

            public List<int> SinavIds { get; } = new();
            public int OgrenciSayisi { get; private set; }

            public Run(DuzceUniversiteContext context, byte[] hash, byte[] salt)
            {
                _c = context;
                _hash = hash;
                _salt = salt;
            }

            public async Task SeedAllAsync()
            {
                await SeedBolumlerAsync();
                await SeedDersliklerAsync();
                await SeedDerslerAsync();
                await SeedHesaplarAsync();
                await SeedOgrencilerAsync();
                await SeedMusaitlikAsync();
                var atamalar = await SeedAtamalarAsync();
                await SeedSinavlarAsync(atamalar);
                await SeedDuyurularAsync();
            }

            private async Task SeedBolumlerAsync()
            {
                foreach (var b in Bolumler)
                {
                    var bolum = await _c.Bolum.FirstOrDefaultAsync(x => x.Ad == b.Ad);
                    if (bolum == null)
                    {
                        bolum = new Bolum { Ad = b.Ad, Status = true, CreatedDate = _now };
                        _c.Bolum.Add(bolum);
                        await _c.SaveChangesAsync();
                    }
                    _bolum[b.Kisa] = bolum.Id;
                }
            }

            private async Task SeedDersliklerAsync()
            {
                foreach (var d in Derslikler)
                {
                    var derslik = await _c.Derslik.FirstOrDefaultAsync(x => x.Ad == d.Ad);
                    if (derslik == null)
                    {
                        derslik = new Derslik { Ad = d.Ad, Kapasite = d.Kapasite, Status = d.Aktif, CreatedDate = _now };
                        _c.Derslik.Add(derslik);
                        await _c.SaveChangesAsync();
                        foreach (var b in d.Bolumler)
                            _c.DerslikBolum.Add(new DerslikBolum { DerslikId = derslik.Id, BolumId = _bolum[b], Status = true, CreatedDate = _now });
                        await _c.SaveChangesAsync();
                    }
                    if (derslik.Status)
                    {
                        var bolumIds = await _c.DerslikBolum.Where(x => x.DerslikId == derslik.Id).Select(x => x.BolumId).ToListAsync();
                        var kisa = _bolum.Where(kv => bolumIds.Contains(kv.Value)).Select(kv => kv.Key).ToArray();
                        _derslik[d.Ad] = (derslik.Id, derslik.Kapasite, kisa);
                    }
                }
            }

            private async Task SeedDerslerAsync()
            {
                foreach (var d in Dersler)
                {
                    var ders = await _c.Ders.FirstOrDefaultAsync(x => x.Kod == d.Kod);
                    if (ders == null)
                    {
                        ders = new Ders { Kod = d.Kod, Ad = d.Ad, Status = true, CreatedDate = _now };
                        _c.Ders.Add(ders);
                        await _c.SaveChangesAsync();
                        foreach (var b in d.Bolumler)
                            _c.DersBolum.Add(new DersBolum { DersId = ders.Id, BolumId = _bolum[b], Status = true, CreatedDate = _now });
                        await _c.SaveChangesAsync();
                    }
                    _ders[d.Kod] = ders.Id;
                }
            }

            private async Task<User> AddUserAsync(string firstName, string lastName, string userName, string email, params string[] roles)
            {
                // Aynı kullanıcı adı veya e-posta varsa sonuna rakam eklenir
                var baseName = userName;
                for (var i = 2; await _c.Users.AnyAsync(u => u.UserName == userName || u.Email == email); i++)
                {
                    userName = $"{baseName}{i}";
                    email = $"{userName}@{email.Split('@')[1]}";
                }

                var user = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    UserName = userName,
                    Email = email,
                    PasswordHash = _hash,
                    PasswordSalt = _salt,
                    MustChangePassword = false,
                    Status = true,
                    CreatedDate = _now,
                };
                _c.Users.Add(user);
                await _c.SaveChangesAsync();
                foreach (var role in roles)
                    _c.UserOperationClaims.Add(new UserOperationClaim
                    {
                        UserId = user.Id,
                        OperationClaimId = await _c.GetRoleIdAsync(role),
                        Status = true,
                        CreatedDate = _now,
                    });
                await _c.SaveChangesAsync();
                return user;
            }

            private async Task SeedHesaplarAsync()
            {
                var admin = await AddUserAsync("Demo", "Yönetici", MarkerUserName, $"{MarkerUserName}@{EmailDomain}", Roles.Admin);
                _adminUserId = admin.Id;
                await AddUserAsync("Nihat", "Sarı", "fakulte.sekreteri", $"fakulte.sekreteri@{EmailDomain}", Roles.GorevliPersonel);

                foreach (var p in Personeller)
                {
                    var (first, last) = AccountFactory.SplitFullName(p.Ad);
                    var userName = AccountFactory.ToAsciiLower($"{first.Replace(" ", "")}.{last}");
                    var gorevli = Gorevliler.Any(g => g.Personel == p.Ad);
                    var roles = gorevli ? new[] { Roles.AkademikPersonel, Roles.BolumGorevlisi } : new[] { Roles.AkademikPersonel };
                    var user = await AddUserAsync(first, last, userName, $"{userName}@{EmailDomain}", roles);
                    var personel = new AkademikPersonel { Ad = p.Ad, Unvan = p.Unvan, UserId = user.Id, Status = true, CreatedDate = _now };
                    _c.AkademikPersonel.Add(personel);
                    await _c.SaveChangesAsync();
                    _personel[p.Ad] = (personel.Id, user.Id, p.Unvan, p.Bolum);
                }

                foreach (var (bolum, ad) in Gorevliler)
                    _c.BolumAkademikPersoneller.Add(new BolumAkademikPersoneller
                    {
                        BolumId = _bolum[bolum],
                        AkademikPersonelId = _personel[ad].Id,
                        Status = true,
                        CreatedDate = _now,
                    });
                await _c.SaveChangesAsync();
            }

            private async Task SeedOgrencilerAsync()
            {
                var role = await _c.GetRoleIdAsync(Roles.Ogrenci);
                var existingNos = (await _c.Ogrenci.Select(o => o.OgrenciNo).ToListAsync()).ToHashSet();
                var existingUsers = (await _c.Users.Select(u => u.UserName).ToListAsync()).ToHashSet();
                for (var bi = 0; bi < Bolumler.Length; bi++)
                {
                    for (var sinif = 1; sinif <= 4; sinif++)
                    {
                        for (var n = 1; n <= 10; n++)
                        {
                            // Öğrenci no: giriş yılı + bölüm sırası + sıra (ör. 2025010003)
                            var no = $"{2026 - sinif}{bi + 1:00}{sinif * 100 + n:0000}";
                            if (existingNos.Contains(no) || existingUsers.Contains(no))
                                continue;
                            var kadin = _random.Next(2) == 0;
                            var ad = kadin ? KadinAdlari[_random.Next(KadinAdlari.Length)] : ErkekAdlari[_random.Next(ErkekAdlari.Length)];
                            var soyad = Soyadlari[_random.Next(Soyadlari.Length)];
                            // Her bölümün 4. sınıfının 10. öğrencisi pasif: giriş yapamaz
                            var aktif = !(sinif == 4 && n == 10);
                            var user = new User
                            {
                                FirstName = ad, LastName = soyad, UserName = no, Email = $"{no}@ogr.{EmailDomain}",
                                PasswordHash = _hash, PasswordSalt = _salt, MustChangePassword = false, Status = aktif, CreatedDate = _now,
                            };
                            _c.Users.Add(user);
                            await _c.SaveChangesAsync();
                            _c.UserOperationClaims.Add(new UserOperationClaim { UserId = user.Id, OperationClaimId = role, Status = true, CreatedDate = _now });
                            _c.Ogrenci.Add(new Ogrenci
                            {
                                UserId = user.Id, OgrenciNo = no, Ad = ad, Soyad = soyad, BolumId = _bolum[Bolumler[bi].Kisa],
                                Sinif = sinif, Status = aktif, CreatedDate = _now,
                            });
                            OgrenciSayisi++;
                        }
                    }
                }
                await _c.SaveChangesAsync();
            }

            private DateTime NextWeekday(DateTime from, DayOfWeek day)
            {
                var d = from.Date;
                while (d.DayOfWeek != day) d = d.AddDays(1);
                return d;
            }

            private async Task SeedMusaitlikAsync()
            {
                var today = _now.Date;
                AkademikPersonelMusaitlik Kayit(string personel, DateTime bas, DateTime? bit, TimeSpan? saatBas, TimeSpan? saatBit,
                    TekrarTipiEnum tekrar, bool musait, string neden, string? aciklama = null) => new()
                {
                    AkademikPersonelId = _personel[personel].Id,
                    BaslangicTarihi = bas,
                    BitisTarihi = bit,
                    BaslangicSaati = saatBas,
                    BitisSaati = saatBit,
                    TekrarTipi = tekrar,
                    TekrarGunu = tekrar switch
                    {
                        TekrarTipiEnum.Haftalik => (int)bas.DayOfWeek,
                        TekrarTipiEnum.Aylik => bas.Day,
                        _ => null,
                    },
                    IsMusait = musait,
                    Neden = neden,
                    Aciklama = aciklama,
                    Status = true,
                    CreatedDate = _now,
                };

                var gun3 = WorkDays(today, 12)[2];
                var gun5 = WorkDays(today, 12)[4];
                _musaitlik.AddRange(new[]
                {
                    Kayit("Mehmet Demir", NextWeekday(today, DayOfWeek.Wednesday), null, new(13, 0, 0), new(17, 0, 0),
                        TekrarTipiEnum.Haftalik, false, "Ders", "Lisansüstü ders saati"),
                    Kayit("Gizem Yıldız", NextWeekday(today, DayOfWeek.Monday), null, new(9, 0, 0), new(12, 30, 0),
                        TekrarTipiEnum.Haftalik, false, "Laboratuvar dersi"),
                    Kayit("Selin Aydın", gun3, null, null, null, TekrarTipiEnum.Tekil, false, "Konferans", "Ulusal bilgisayar kongresi"),
                    Kayit("Emre Polat", gun5, gun5.AddDays(1), null, null, TekrarTipiEnum.Tekil, false, "Yıllık izin"),
                    Kayit("Can Erdoğan", new DateTime(today.Year, today.Month, 1).AddMonths(today.Day > 15 ? 1 : 0).AddDays(14), null,
                        null, null, TekrarTipiEnum.Aylik, false, "Bölüm kurulu"),
                    Kayit("Ece Güneş", NextWeekday(today, DayOfWeek.Thursday), null, new(9, 0, 0), new(17, 0, 0),
                        TekrarTipiEnum.Haftalik, true, "Gözetmenlik tercih", "Perşembe günleri görev alabilirim"),
                });
                _c.AkademikPersonelMusaitlik.AddRange(_musaitlik);
                await _c.SaveChangesAsync();
            }

            private async Task<List<(int Id, string Kod, string Bolum, string Personel)>> SeedAtamalarAsync()
            {
                var result = new List<(int, string, string, string)>();
                foreach (var a in Atamalar)
                {
                    var dbap = new DersBolumAkademikPersonel
                    {
                        DersId = _ders[a.Kod],
                        BolumId = _bolum[a.Bolum],
                        AkademikPersonelId = _personel[a.Personel].Id,
                        Status = true,
                        CreatedDate = _now,
                    };
                    _c.DersBolumAkademikPersonel.Add(dbap);
                    await _c.SaveChangesAsync();
                    result.Add((dbap.Id, a.Kod, a.Bolum, a.Personel));
                }
                return result;
            }

            private static List<DateTime> WorkDays(DateTime from, int count)
            {
                var days = new List<DateTime>();
                for (var d = from.Date.AddDays(1); days.Count < count; d = d.AddDays(1))
                    if (d.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
                        days.Add(d);
                return days;
            }

            /// <summary>Uygulamanın müsaitlik kuralıyla aynı: kayıt o gün ve saat aralığını kapsıyor mu?</summary>
            private bool MusaitDegil(int personelId, DateTime gun, TimeOnly bas, TimeOnly bit) =>
                _musaitlik.Any(m =>
                {
                    if (m.AkademikPersonelId != personelId || m.IsMusait) return false;
                    if (gun < m.BaslangicTarihi.Date) return false;
                    var gunTutar = m.TekrarTipi switch
                    {
                        TekrarTipiEnum.Tekil => gun <= (m.BitisTarihi ?? m.BaslangicTarihi).Date,
                        TekrarTipiEnum.Haftalik => (m.BitisTarihi == null || gun <= m.BitisTarihi.Value.Date) && (int)gun.DayOfWeek == m.TekrarGunu,
                        TekrarTipiEnum.Aylik => (m.BitisTarihi == null || gun <= m.BitisTarihi.Value.Date) &&
                                               gun.Day == Math.Min(m.TekrarGunu ?? 0, DateTime.DaysInMonth(gun.Year, gun.Month)),
                        _ => false,
                    };
                    if (!gunTutar) return false;
                    if (m.BaslangicSaati == null || m.BitisSaati == null) return true;
                    return bas.ToTimeSpan() < m.BitisSaati && bit.ToTimeSpan() > m.BaslangicSaati;
                });

            /// <summary>
            /// Ara sınav dönemi: bu haftanın pazartesisinden itibaren 15 iş günü (takvim açılınca dolu görünsün), günde 4 oturum. Aynı bölümün aynı oturumda iki sınavı,
            /// aynı derslik/öğretim elemanı/gözetmenin çakışması ve "müsait değil" gözetmen olmaz.
            /// Mevcut sınavlar da hesaba katılır. Birkaç dersliğe bilerek gözetmen atanmaz (uyarı görünsün).
            /// </summary>
            private async Task SeedSinavlarAsync(List<(int Id, string Kod, string Bolum, string Personel)> atamalar)
            {
                var pazartesi = _now.Date.AddDays(-(((int)_now.DayOfWeek + 6) % 7));
                var gunler = WorkDays(pazartesi.AddDays(-1), 15);
                var ilk = gunler[0];
                var son = gunler[^1];

                // (gün, oturum) → dolu derslik / personel / bölüm
                var dolu = new HashSet<string>();
                string Key(string tur, object id, DateTime gun, int oturum) => $"{tur}:{id}:{gun:yyyyMMdd}:{oturum}";

                // Mevcut sınavlar: kesişen oturumları dolu say
                var mevcut = await (from s in _c.SinavDetay
                                    join d in _c.DersBolumAkademikPersonel on s.DersBolumAkademikPersonelId equals d.Id
                                    where s.SinavTarihi >= ilk && s.SinavTarihi <= son.AddDays(1)
                                    select new { s.Id, s.SinavTarihi, s.SinavBaslangicSaati, s.SinavBitisSaati, d.AkademikPersonelId, d.BolumId }).ToListAsync();
                var mevcutIds = mevcut.Select(m => m.Id).ToList();
                var mevcutDerslik = await _c.SinavDerslik.Where(x => mevcutIds.Contains(x.SinavDetayId)).ToListAsync();
                foreach (var m in mevcut)
                    for (var o = 0; o < Oturumlar.Length; o++)
                    {
                        if (!(m.SinavBaslangicSaati < Oturumlar[o].Bit && m.SinavBitisSaati > Oturumlar[o].Bas)) continue;
                        var gun = m.SinavTarihi.Date;
                        dolu.Add(Key("p", m.AkademikPersonelId, gun, o));
                        foreach (var sd in mevcutDerslik.Where(x => x.SinavDetayId == m.Id))
                        {
                            dolu.Add(Key("d", sd.DerslikId, gun, o));
                            if (sd.GozetmenId != null) dolu.Add(Key("p", sd.GozetmenId.Value, gun, o));
                        }
                    }

                var gozetmenYuku = _personel.Values.ToDictionary(p => p.Id, _ => 0);
                var bolumGunluk = new Dictionary<string, int>();
                // Ortak dersler önce (bütün bölümleri ilgilendirir), sonra bölüm dersleri
                var sirali = atamalar.OrderBy(a => Dersler.First(d => d.Kod == a.Kod).Bolumler.Length == 0 ? 0 : 1)
                    .ThenBy(a => a.Kod).ThenBy(a => a.Bolum).ToList();
                var sayac = 0;

                foreach (var a in sirali)
                {
                    var ogretmen = _personel[a.Personel];
                    var gerekli = Dersler.First(d => d.Kod == a.Kod).Bolumler.Length == 0 ? 45 : 40;
                    var uygunDerslikler = _derslik
                        .Where(d => d.Value.Bolumler.Length == 0 || d.Value.Bolumler.Contains(a.Bolum))
                        .OrderBy(d => d.Value.Bolumler.Length == 0 ? 1 : 0).ThenBy(d => d.Value.Kapasite).ToList();

                    var yerlesti = false;
                    // Bölümlere göre farklı günden başla ki sınavlar iki haftaya yayılsın
                    var baslangic = (Array.FindIndex(Bolumler, b => b.Kisa == a.Bolum) * 2 + sayac++) % gunler.Count;
                    for (var k = 0; k < gunler.Count * Oturumlar.Length && !yerlesti; k++)
                    {
                        var gun = gunler[(baslangic + k / Oturumlar.Length) % gunler.Count];
                        // Oturumlar da dönüşümlü: sınavlar sabaha yığılmasın
                        var o = (k + sayac) % Oturumlar.Length;
                        var (bas, bit) = Oturumlar[o];
                        if (dolu.Contains(Key("b", a.Bolum, gun, o))) continue;
                        if (bolumGunluk.GetValueOrDefault($"{a.Bolum}:{gun:yyyyMMdd}") >= 2) continue;
                        if (dolu.Contains(Key("p", ogretmen.Id, gun, o))) continue;

                        // Kapasite yetene kadar en küçük uygun derslikler
                        var secilen = new List<(string Ad, int Id)>();
                        var toplam = 0;
                        var tek = uygunDerslikler.FirstOrDefault(d => d.Value.Kapasite >= gerekli && !dolu.Contains(Key("d", d.Value.Id, gun, o)));
                        if (tek.Key != null)
                        {
                            secilen.Add((tek.Key, tek.Value.Id));
                            toplam = tek.Value.Kapasite;
                        }
                        else
                        {
                            foreach (var d in uygunDerslikler.Where(d => !dolu.Contains(Key("d", d.Value.Id, gun, o))))
                            {
                                secilen.Add((d.Key, d.Value.Id));
                                toplam += d.Value.Kapasite;
                                if (toplam >= gerekli) break;
                            }
                        }
                        if (toplam < gerekli) continue;

                        // Gözetmenler: görev yükü en az olan, eşitlikte aynı bölümden ve araştırma görevlisi olan
                        var gozetmenler = new List<int?>();
                        var bu = new HashSet<int> { ogretmen.Id };
                        // BLM305 ve EEM203'te gözetmen bilerek atanmaz: ana sayfada "gözetmen eksik" uyarısı görünsün
                        var gozetmensizBirak = a.Kod is "BLM305" or "EEM203";
                        foreach (var _ in secilen)
                        {
                            var aday = gozetmensizBirak ? null : _personel.Values
                                .Where(p => !bu.Contains(p.Id) && !dolu.Contains(Key("p", p.Id, gun, o)) && !MusaitDegil(p.Id, gun, bas, bit))
                                .OrderBy(p => gozetmenYuku[p.Id])
                                .ThenBy(p => p.Bolum == a.Bolum ? 0 : 1)
                                .ThenBy(p => p.Unvan == "Arş. Gör." ? 0 : 1)
                                .Select(p => (int?)p.Id)
                                .FirstOrDefault();
                            if (aday != null)
                            {
                                bu.Add(aday.Value);
                                gozetmenYuku[aday.Value]++;
                            }
                            gozetmenler.Add(aday);
                        }

                        var sinav = new SinavDetay
                        {
                            DersBolumAkademikPersonelId = a.Id,
                            SinavTarihi = gun,
                            SinavBaslangicSaati = bas,
                            SinavBitisSaati = bit,
                            Status = true,
                            CreatedDate = _now,
                        };
                        _c.SinavDetay.Add(sinav);
                        await _c.SaveChangesAsync();
                        for (var i = 0; i < secilen.Count; i++)
                        {
                            _c.SinavDerslik.Add(new SinavDerslik
                            {
                                SinavDetayId = sinav.Id,
                                DerslikId = secilen[i].Id,
                                GozetmenId = gozetmenler[i],
                                Status = true,
                                CreatedDate = _now,
                            });
                            dolu.Add(Key("d", secilen[i].Id, gun, o));
                            if (gozetmenler[i] != null) dolu.Add(Key("p", gozetmenler[i]!.Value, gun, o));
                        }
                        await _c.SaveChangesAsync();

                        dolu.Add(Key("p", ogretmen.Id, gun, o));
                        // Ortak ders sınavı o bölümün öğrencilerini de bağlar
                        dolu.Add(Key("b", a.Bolum, gun, o));
                        bolumGunluk[$"{a.Bolum}:{gun:yyyyMMdd}"] = bolumGunluk.GetValueOrDefault($"{a.Bolum}:{gun:yyyyMMdd}") + 1;
                        SinavIds.Add(sinav.Id);
                        yerlesti = true;
                    }
                }
            }

            private async Task SeedDuyurularAsync()
            {
                Announcement Duyuru(string baslik, string icerik, string tur, int oncelik, string hedef, string? bolum,
                    int yayinGunOnce, int? bitisGunSonra, bool popup) => new()
                {
                    Title = baslik,
                    Content = icerik,
                    Type = tur,
                    Priority = oncelik,
                    TargetAudience = hedef,
                    TargetBolumId = bolum == null ? null : _bolum[bolum],
                    PublishDate = _now.AddDays(-yayinGunOnce),
                    ExpiryDate = bitisGunSonra == null ? null : _now.Date.AddDays(bitisGunSonra.Value),
                    IsActive = true,
                    ShowAsPopup = popup,
                    CreatedBy = _adminUserId,
                    Status = true,
                    CreatedDate = _now,
                };

                _c.Announcements.AddRange(
                    Duyuru("Güz yarıyılı ara sınav takvimi yayınlandı",
                        "Ara sınavlar bu hafta başladı ve üç hafta sürecektir. Kendi sınavlarınızı Sınav Takvimi ekranından, " +
                        "ana sayfadaki listeden ve PDF olarak indirebileceğiniz resmi takvimden görebilirsiniz.",
                        "general", 1, AnnouncementAudiences.All, null, 1, 21, popup: true),
                    Duyuru("Sınav kuralları",
                        "Sınava öğrenci kimlik kartınızla, sınav saatinden 15 dakika önce gelin. Cep telefonları kapalı ve " +
                        "çantada olmalıdır. Sınavın ilk 20 dakikası salondan çıkılamaz.",
                        "general", 0, AnnouncementAudiences.Ogrenci, null, 1, 21, popup: false),
                    Duyuru("Gözetmen görevlendirmeleri",
                        "Gözetmenlik görevleriniz Sınav Takvimi'nde 'Sadece benim sınavlarım' ile listelenir. Müsait olmadığınız " +
                        "zamanları Müsaitlik ekranından girin; bu saatlerde size görev verilmez.",
                        "event", 1, AnnouncementAudiences.Personel, null, 2, 21, popup: false),
                    Duyuru("BLM-LAB1 bakımda",
                        "Bilgisayar laboratuvarı Cuma günü 13:00-17:00 arasında bakımda olacaktır.",
                        "maintenance", 0, AnnouncementAudiences.All, "BLM", 0, 7, popup: false),
                    Duyuru("AMFI-1 klima arızası",
                        "AMFI-1'deki sınavlarda salon sıcak olabilir; gözetmenlerin pencereleri açık tutması rica olunur.",
                        "urgent", 2, AnnouncementAudiences.Personel, null, 0, 3, popup: false));
                await _c.SaveChangesAsync();
            }
        }
    }
}
