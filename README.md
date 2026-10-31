# Sinav-Takvimi-Backend

Düzce Üniversitesi sınav takvimi planlama sistemi için ASP.NET Core 8 Web API (`API/`).
Katmanlar: `Core` → `Entity` → `DataAccess` → `Business` → `API`. Birim testleri `Tests/` altındadır.

Arayüz: [sinav-takvimi-frontend](https://github.com/yasirsharp/sinav-takvimi-frontend). Sunucuda derlenmiş
arayüz `API/wwwroot` içinden sunulur; arayüz ve API aynı adresten çalışır.

**Sunucuya kurulum (IIS + SQL Server):** [docs/KURULUM.md](docs/KURULUM.md) ·
**Plesk (Windows) + MySQL test kurulumu:** [docs/PLESK_KURULUM.md](docs/PLESK_KURULUM.md) ·
**Test sitesine otomatik yükleme (GitHub Actions, FTP):** [docs/PLESK_KURULUM.md → Otomatik güncelleme](docs/PLESK_KURULUM.md#otomatik-güncelleme-github-actions)

> `Frontend1/` eski MVC arayüzüdür ve artık kullanılmamaktadır.

## Kurulum

Gizli bilgiler (JWT anahtarı, veritabanı parolası) repoda **tutulmaz**. Uygulama, JWT anahtarı
tanımlı değilse veya 32 bayttan kısaysa açılmaz.

### Geliştirme ortamı (User Secrets)

```bash
cd API
dotnet user-secrets set "TokenOptions:SecurityKey" "$(openssl rand -base64 64)"
# LocalDB dışında bir SQL Server kullanılıyorsa:
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True"
dotnet run
```

Boş bir geliştirme veritabanı ve ilk yönetici için:

```bash
dotnet tool restore
dotnet ef database update --project DataAccess --startup-project DataAccess   # ConnectionStrings__DefaultConnection ortam değişkeninden
cd API
dotnet user-secrets set "Seed:AdminEmail" "admin@duzce.edu.tr"
dotnet user-secrets set "Seed:AdminPassword" "GeciciParola1"   # ilk girişte değiştirilir
```

### Sunucu ortamı (ortam değişkenleri)

| Değişken | Açıklama |
|---|---|
| `TokenOptions__SecurityKey` | En az 64 karakterlik rastgele değer |
| `ConnectionStrings__DefaultConnection` | SQL Server bağlantı dizesi |
| `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, ... | Sadece frontend ayrı bir adresten sunuluyorsa gerekir |
| `Seed__AdminEmail`, `Seed__AdminPassword` | Hiç super.admin yoksa açılışta ilk yöneticiyi oluşturur |

Diğer ayarlar (açık kayıt, açılışta migration, log yolu) için [docs/KURULUM.md](docs/KURULUM.md).

## Hesaplar

- Herkese açık kayıt kapalıdır (`Auth:AllowPublicRegistration=false`; `POST /api/Auth/register` 404 döner).
- Hesapları yönetici açar: `POST /api/AkademikPersonel`, `POST /api/Ogrenci` ve toplu olarak
  `POST /api/AkademikPersonel/bulk`, `POST /api/Ogrenci/bulk` (en fazla 1000 satır, satır bazında sonuç).
  Her hesaba rastgele bir geçici parola üretilir ve yanıtta bir kez döner.
- Geçici parolalı hesabın token'ında `pwd_change` claim'i bulunur. Kullanıcı `POST /api/Auth/change-password`
  ile parolasını değiştirene kadar `GET /api/User/me` dışındaki tüm uçlar 403 (`PASSWORD_CHANGE_REQUIRED`) döner.
- Yönetici `POST /api/User/{id}/reset-password` ile geçici parola üretebilir. super.admin hesaplarını yalnızca
  super.admin değiştirebilir, sıfırlayabilir veya silebilir.
- Öğrencinin kullanıcı adı öğrenci numarasıdır. Öğrenciler sadece kendi bölümlerinin sınavlarını görür
  (`GET /api/SinavDetay/daterange-bolum`, bölüm parametresi verilmezse token'daki bölüm kullanılır).

## Yetkilendirme

- `[AllowAnonymous]` olmayan her uç nokta geçerli bir JWT ister (`FallbackPolicy`).
- Rol grupları `Business/Constants/Roles.cs` içinde tanımlıdır:
  - `Yonetici` (super.admin, Admin): kullanıcı, yetki, bölüm, akademik personel ve öğrenci yönetimi
  - `Planlayici` (Yonetici + bolum.gorevlisi): ders, derslik, DBAP ve sınav ekleme/güncelleme/silme
  - `Personel` (Planlayici + gorevli.personel + akademik.personel): öğrenci listesi, müsaitlik
- Kullanıcıya özel uç noktalar (bildirim, asistan geçmişi/tercihleri, duyuru okundu bilgisi,
  müsaitlik kayıtları) kullanıcı ID'sini token'dan doğrular; başka kullanıcının verisine erişim 403 döner.
- `POST /api/Auth/login`, `register` ve `change-password` IP başına dakikada 10 istekle sınırlıdır.

## Sınav planlama kuralları

Aynı gün ve kesişen saatlerde: aynı derslik iki sınava verilemez; bir gözetmen iki sınavda olamaz; bir öğretim
elemanının iki sınavı olamaz; gözetmen, başka bir sınavın dersini veriyor olamaz (kendi sınavında gözetmen
olabilir). Müsaitlik takviminde "müsait değil" olan personel gözetmen seçilemez. Kontrol ve kayıt tek
transaction içinde ve uygulama kilidiyle (`sp_getapplock`) yapılır.

Sınav eklenince, değişince veya silinince dersin öğretim elemanına ve gözetmenlere bildirim gider.

## Veritabanı

Varsayılan veritabanı SQL Server'dır. Test sunucuları için MySQL 8 / MariaDB 10.5+ da desteklenir
(`Database:Provider=MySql`). Sınav planlama kilidi SQL Server'da `sp_getapplock`, MySQL'de `GET_LOCK` ile yapılır.

Şema EF Core migration'larıyla yönetilir: SQL Server `DataAccess/Migrations`, MySQL `DataAccess.MySql/Migrations`. Roller migration içinde sabit ID'lerle
tohumlanır; kod rollere adlarıyla erişir.

- Sunucu kurulumu ve güncelleme: `db/schema.sql` / `db/schema.mysql.sql` (idempotent, tekrar çalıştırmak
  güvenlidir) veya `Database:MigrateOnStartup=true`.
- Model değişince:

```bash
dotnet ef migrations add <Ad> --project DataAccess --startup-project DataAccess --output-dir Migrations
dotnet ef migrations script --idempotent --project DataAccess --startup-project DataAccess -o db/schema.sql
# MySQL için de:
dotnet ef migrations add <Ad> --project DataAccess.MySql --startup-project DataAccess.MySql --output-dir Migrations
dotnet ef migrations script --idempotent --project DataAccess.MySql --startup-project DataAccess.MySql -o db/schema.mysql.sql
```

`db-scripts/` altındaki eski betikler yeni kurulumda kullanılmaz.

## Testler

```bash
dotnet test Tests
```

CI (`.github/workflows/ci.yml`) derleme, birim testleri ve migration'ın modelle uyumunu kontrol eder.
