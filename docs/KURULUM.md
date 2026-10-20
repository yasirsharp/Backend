# Kurulum (Windows Server / IIS)

Bu belge, Sınav Takvimi'ni üniversite sunucusunda **tek bir IIS sitesi** olarak çalıştırmayı anlatır.
Arayüz (React) ve API aynı adresten sunulur: `https://sinav.duzce.edu.tr/` arayüz, `/api/...` API'dir.
Bu yüzden CORS ayarı gerekmez.

## 1. Gerekenler

| Nerede | Ne |
|---|---|
| Sunucu | Windows Server 2019+ ve IIS |
| Sunucu | [.NET 8 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/8.0) (kurulumdan sonra `iisreset`) |
| Veritabanı | SQL Server 2017+ (Express de olur) |
| Paketi hazırlayan bilgisayar | .NET 8 SDK, Node.js 20+, Git, PowerShell |

## 2. Yayın paketini hazırlama

Backend ve frontend repolarını yan yana klonlayın, sonra backend klasöründe:

```powershell
.\scripts\publish.ps1 -FrontendPath ..\sinav-takvimi-frontend
```

Komut frontend'i derleyip `API\wwwroot`'a kopyalar, `dotnet publish` çalıştırır ve sonucu `publish\` klasörüne
koyar. Klasörde `web.config` (IIS ayarları) ve `schema.sql` (veritabanı betiği) de bulunur.

## 3. Veritabanı

1. SQL Server'da boş bir veritabanı açın (ör. `SinavTakvimi`).
2. `schema.sql` betiğini bu veritabanında çalıştırın (SSMS'te dosyayı açıp **Execute**).
   - Betik tüm tabloları, indeksleri ve rolleri oluşturur.
   - Tekrar çalıştırmak güvenlidir: uygulanmış adımları atlar. Yeni sürümlerde de aynı betik çalıştırılır.
3. Uygulama için ayrı bir SQL kullanıcısı açın; `sa` kullanmayın:

```sql
CREATE LOGIN sinav_app WITH PASSWORD = '<güçlü parola>';
USE SinavTakvimi;
CREATE USER sinav_app FOR LOGIN sinav_app;
ALTER ROLE db_datareader ADD MEMBER sinav_app;
ALTER ROLE db_datawriter ADD MEMBER sinav_app;
```

> `db-scripts/` klasöründeki eski betikler yeni kurulumda **kullanılmaz**.

## 4. IIS sitesi

1. `publish\` klasörünün içeriğini sunucuda örneğin `C:\inetpub\sinav-takvimi` klasörüne kopyalayın.
2. IIS Yöneticisi → **Uygulama Havuzları** → yeni havuz: .NET CLR sürümü **Yönetilen Kod Yok**.
3. **Siteler** → yeni site: fiziksel yol yukarıdaki klasör, uygulama havuzu yeni havuz, HTTPS bağlaması ve
   üniversite sertifikası. HTTP'yi HTTPS'e yönlendirin.
4. `logs` klasörüne uygulama havuzu kullanıcısı (`IIS AppPool\<havuz adı>`) için **yazma** izni verin.

## 5. Ayarlar (ortam değişkenleri)

Gizli bilgiler `appsettings.json`'a yazılmaz. IIS Yöneticisi → site → **Yapılandırma Düzenleyicisi** →
`system.webServer/aspNetCore` → `environmentVariables` üzerinden ekleyin. İsterseniz `web.config`'teki
yorum satırlarını da doldurabilirsiniz. Bu durumda dosyanın okuma iznini kısıtlayın.

| Değişken | Zorunlu | Açıklama |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Evet | `Server=...;Database=SinavTakvimi;User Id=sinav_app;Password=...;TrustServerCertificate=True` |
| `TokenOptions__SecurityKey` | Evet | En az 64 karakter rastgele değer. Değişirse herkesin oturumu kapanır. |
| `Seed__AdminEmail` | İlk kurulumda | İlk yöneticinin e-postası |
| `Seed__AdminPassword` | İlk kurulumda | İlk yöneticinin geçici parolası (en az 8 karakter) |
| `Seed__AdminUserName` | Hayır | Boşsa e-postanın @ öncesi kullanılır |
| `ASPNETCORE_ENVIRONMENT` | Hayır | `web.config`'te `Production` olarak gelir |
| `Auth__AllowPublicRegistration` | Hayır | Varsayılan `false`. Hesapları yönetici açar. |
| `Database__MigrateOnStartup` | Hayır | `true` ise açılışta şemayı kendisi günceller. Varsayılan `false`: şema `schema.sql` ile kurulur. Açmak için uygulama kullanıcısının `db_owner` olması gerekir. |
| `Serilog__FilePath` | Hayır | Log dosyası yolu. Varsayılan `logs\api-.log` (günlük döner, 14 gün saklanır). |
| `RateLimiting__AuthPermitPerMinute` | Hayır | Giriş uçlarına IP başına dakikada izin verilen istek (varsayılan 100). Kampüs NAT/proxy arkasındaki kullanıcılar aynı IP'yi paylaştığı için düşük tutmayın; hesap başına kaba kuvvet koruması ayrıca vardır (15 dakikada 5 hatalı deneme → 15 dakika kilit). |

Rastgele anahtar üretmek için (PowerShell):

```powershell
[Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Maximum 256 }) -as [byte[]])
```

## 6. İlk çalıştırma

1. `https://<adres>/health` → `{"status":"Healthy"}` görünmeli. `Unhealthy` ise bağlantı dizesini kontrol edin.
2. Ana sayfayı açın ve `Seed__AdminEmail` / `Seed__AdminPassword` ile giriş yapın.
   Sistem hemen yeni parola ister.
3. İlk yönetici oluştuktan sonra `Seed__AdminPassword` değişkenini **silin**.
   Hiç aktif super.admin kalmazsa açılışta yeniden oluşturulur.
4. Veri girişi sırası ve toplu içe aktarma şablonları için frontend reposundaki `docs/VERI_GIRISI.md` dosyasına bakın.

## 7. Hesaplar ve parolalar

- Herkese açık kayıt kapalıdır. Kullanıcıları yönetici açar: akademik personel ve öğrenci ekranları ile toplu
  içe aktarma.
- Açılan her hesaba rastgele bir **geçici parola** verilir ve yöneticiye bir kez gösterilir.
  Kullanıcı ilk girişte parolasını değiştirmeden hiçbir işlem yapamaz.
- Öğrencinin kullanıcı adı öğrenci numarasıdır. E-posta verilmezse `<no>@ogr.duzce.edu.tr` atanır.
- Parolasını unutan kullanıcı için: **Yönetim → Kullanıcılar → Parola sıfırla**.
- Parola kuralı: en az 8 karakter, büyük harf, küçük harf ve rakam.

## 8. Güncelleme

1. Yeni paketi hazırlayın (adım 2).
2. Önce veritabanını yedekleyin, sonra yeni `schema.sql`'i çalıştırın.
3. Siteyi durdurun veya klasöre `app_offline.htm` koyun. Dosyaları kopyalayın; `logs` klasörünü ve
   ayarlarınızı içeren `web.config`'i koruyun. Siteyi başlatın.

## 9. Yedekleme

SQL Server Agent ile her gece tam yedek alın (Express sürümünde Windows Görev Zamanlayıcı + `sqlcmd`).
Yedekleri başka bir diske kopyalayın. Uygulama klasöründe kalıcı veri yoktur (loglar hariç).

## 10. Sorun giderme

| Belirti | Bakılacak yer |
|---|---|
| 500.30 / site açılmıyor | `web.config`'te `stdoutLogEnabled="true"` yapın, `logs\stdout_*.log` dosyasını okuyun, sonra tekrar kapatın. Genelde eksik `TokenOptions__SecurityKey` veya bağlantı dizesidir. |
| `/health` → Unhealthy | SQL Server erişimi, güvenlik duvarı, kullanıcı izinleri |
| Hata mesajları | `logs\api-YYYYMMDD.log` |
| "rolü veritabanında bulunamadı" | `schema.sql` çalıştırılmamış |
| Sayfa yenileyince 404 | Site kökü `publish` klasörü olmalı. `wwwroot\index.html` mevcut mu? |

## Bilinen kısıtlar

- Hesap kilidi ve hız sınırı sayaçları bellekte tutulur (tek sunucu için tasarlandı); uygulama havuzu yeniden
  başlayınca sıfırlanır.

- Oturum belirteci (JWT) 4 saat geçerlidir. Pasifleştirilen kullanıcının veya parolası sıfırlanan hesabın
  açık oturumu bu süre dolana kadar çalışmaya devam eder.
