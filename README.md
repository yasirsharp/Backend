# Sinav-Takvimi-Backend

Düzce Üniversitesi sınav takvimi planlama sistemi için ASP.NET Core 8 Web API (`API/`).
Katmanlar: `Core` → `Entity` → `DataAccess` → `Business` → `API`.

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

### Sunucu ortamı (ortam değişkenleri)

| Değişken | Açıklama |
|---|---|
| `TokenOptions__SecurityKey` | En az 64 karakterlik rastgele değer |
| `ConnectionStrings__DefaultConnection` | SQL Server bağlantı dizesi |
| `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, ... | Frontend'in adres(ler)i, ör. `https://sinav.duzce.edu.tr` |

## Yetkilendirme

- `[AllowAnonymous]` olmayan her uç nokta geçerli bir JWT ister (`FallbackPolicy`).
- Rol grupları `Business/Constants/Roles.cs` içinde tanımlıdır:
  - `Yonetici` (super.admin, Admin): kullanıcı, yetki, bölüm, akademik personel ve öğrenci yönetimi
  - `Planlayici` (Yonetici + bolum.gorevlisi): ders, derslik, DBAP ve sınav ekleme/güncelleme/silme
  - `Personel` (Planlayici + gorevli.personel + akademik.personel): öğrenci listesi, müsaitlik
- Kullanıcıya özel uç noktalar (bildirim, asistan geçmişi/tercihleri, duyuru okundu bilgisi,
  müsaitlik kayıtları) kullanıcı ID'sini token'dan doğrular; başka kullanıcının verisine erişim 403 döner.
- `POST /api/Auth/login` ve `POST /api/Auth/register` IP başına dakikada 10 istekle sınırlıdır.

## Veritabanı

Şema değişiklikleri `db-scripts/` altındaki SQL betikleriyle elle uygulanır (EF migration kullanılmıyor).
