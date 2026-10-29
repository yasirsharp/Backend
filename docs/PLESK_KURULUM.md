# Plesk (Windows) + MySQL test kurulumu

Bu rehber, sistemi test için bir Plesk Windows sunucusunda tek bir subdomain altında çalıştırmayı anlatır.
Örnek adres: **https://hafiz.yasirsharp.com.tr**.

Arayüz ve API aynı adresten sunulur: `https://hafiz.yasirsharp.com.tr/` arayüz, `/api/...` API'dir.
Üniversitedeki asıl kurulum için [KURULUM.md](KURULUM.md) dosyasına bakın (IIS + SQL Server).

## Hazırlık listesi

| # | Nerede | Ne yapılacak | Süre |
|---|---|---|---|
| 1 | Plesk → Araçlar ve Ayarlar | .NET 8 (ASP.NET Core) desteği | 5 dk |
| 2 | Plesk → Web Siteleri ve Alan Adları | Subdomain + SSL | 5 dk |
| 3 | Plesk → Veritabanları | MySQL veritabanı ve kullanıcısı | 2 dk |
| 4 | Plesk → Dosya Yöneticisi | Zip'i yükleyip çıkarma | 5 dk |
| 5 | Dosya Yöneticisi → web.config | 5 bilgiyi yazma | 2 dk |
| 6 | Tarayıcı | İlk giriş ve veri girişi | — |

## 1. .NET 8 desteği (sunucuda bir kez)

Uygulama ASP.NET Core 8 ile yazıldı. Sunucuda **ASP.NET Core 8 Runtime (Hosting Bundle)** kurulu olmalı.

- **Plesk ile:** Araçlar ve Ayarlar → Güncellemeler → **Bileşen Ekle/Kaldır** → "Web barındırma" altında
  **.NET Core / ASP.NET Core** bileşeninde 8.0 sürümünü seçip kurun.
- **Plesk'te 8.0 yoksa:** sunucuya Uzak Masaüstü ile bağlanın. Microsoft'un sitesinden
  "ASP.NET Core 8.0 Runtime – Windows Hosting Bundle" dosyasını indirip kurun. Ardından komut satırında
  `iisreset` çalıştırın.

Kontrol için sunucuda komut satırında `dotnet --list-runtimes` çalıştırın. Çıktıda `Microsoft.AspNetCore.App 8.0.x`
görünmeli.

## 2. Subdomain ve SSL

1. **Web Siteleri ve Alan Adları** → **Subdomain ekle**.
2. Ad olarak `hafiz` girin; belge kökü `hafiz.yasirsharp.com.tr` gibi kalabilir.
3. Subdomain'in **SSL/TLS Sertifikaları** sayfasından Let's Encrypt sertifikası alın.
4. **Barındırma Ayarları**nda "HTTP'den HTTPS'ye kalıcı 301 yönlendirme"yi açın.
5. **Özel IIS Uygulama Havuzu** sayfasında havuzu açın (önerilir).
6. Barındırma ayarlarında "ASP.NET desteği"ni kapatabilirsiniz. Gerekmez, ama açık kalması çalışmayı da
   engellemez.

## 3. MySQL veritabanı

1. **Veritabanları** → **Veritabanı Ekle**.
2. Türü **MySQL**. Ad, kullanıcı adı ve güçlü bir parola belirleyin.
3. Şunları not edin. Plesk ada önek ekleyebilir, ekranda yazanı kullanın:
   - Veritabanı adı
   - Kullanıcı adı ve parola
   - Veritabanı sunucusu (genelde `localhost:3306`)

Tabloları elle oluşturmanız gerekmez. Uygulama ilk açılışta şemayı kendisi kurar (`Database__MigrateOnStartup=true`).
Elle kurmak isterseniz phpMyAdmin'de `schema.mysql.sql` dosyasını içe aktarın; dosya tekrar çalıştırmaya
güvenlidir.

## 4. Dosyaları yükleme

1. **Dosya Yöneticisi**'nde subdomain'in belge kökünü açın (ör. `hafiz.yasirsharp.com.tr`).
2. Klasörde Plesk'in koyduğu varsayılan sayfalar varsa (`index.html`, `default.htm` vb.) silin.
3. `sinav-takvimi-hafiz.zip` dosyasını **Yükle** ile gönderin.
4. Zip'e sağ tıklayın → **Arşivden çıkar**. `API.dll`, `web.config` ve `wwwroot` klasörü doğrudan belge kökünde
   olmalı, alt klasörde değil.
5. Zip dosyasını silebilirsiniz.

## 5. web.config'i doldurma

Dosya Yöneticisi'nde `web.config` → **Metin düzenleyicide düzenle**. Büyük harfli yer tutucuları değiştirin:

| Yer tutucu | Ne yazılacak |
|---|---|
| `VERITABANI_ADI` | Adım 3'teki veritabanı adı |
| `VERITABANI_KULLANICISI` | Adım 3'teki kullanıcı adı |
| `VERITABANI_PAROLASI` | Adım 3'teki parola |
| `YONETICI_EPOSTASI` | İlk yönetici hesabınızın e-postası (giriş için kullanacaksınız) |
| `YONETICI_GECICI_PAROLASI` | İlk giriş parolası (en az 8 karakter; ilk girişte değiştireceksiniz) |

- Veritabanı sunucusu `localhost:3306` değilse bağlantıdaki `Server=` ve `Port=` değerlerini düzeltin.
- **Güvenlik anahtarı** size gönderilen pakette rastgele üretilmiş olarak gelir; değiştirmeyin.
- Parola özel karakter içeriyorsa:
  - `;` kullanmayın.
  - `&` yerine `&amp;`, `<` yerine `&lt;`, `"` yerine `&quot;` yazın (XML dosyası).

Kaydettiğinizde IIS uygulamayı kendisi yeniden başlatır.

## 6. İlk açılış

1. **https://hafiz.yasirsharp.com.tr/health** adresi `{"status":"Healthy"}` göstermeli. İlk açılış, şema
   kurulduğu için 10–20 saniye sürebilir.
2. **https://hafiz.yasirsharp.com.tr** adresinden yönetici e-postası ve geçici parolayla girin. Sistem hemen yeni
   parola ister.
3. Girişten sonra `web.config`'teki `Seed__AdminPassword` satırını silin. Hiç yönetici kalmazsa uygulama açılışta
   tekrar oluşturur.
4. Verileri girin: ana sayfadaki "Kurulum adımları" sırasıyla, her ekranda **Excel'den aktar**. Ayrıntılar
   arayüz reposundaki `docs/VERI_GIRISI.md` dosyasında.
5. Test kullanıcılarına adresi, hesap bilgilerini ve arayüz reposundaki `docs/TEST_REHBERI.md` dosyasını
   iletin.

## Sorun giderme

| Belirti | Çözüm |
|---|---|
| **500.19** | web.config okunamıyor: ASP.NET Core Hosting Bundle kurulu değil (adım 1). |
| **500.30 / 502.5** (uygulama başlamadı) | `web.config`'te `stdoutLogEnabled="true"` yapın, sayfayı yenileyin, `logs\stdout_*.log` dosyasını okuyun. Sonra tekrar `false` yapın. Mesaj genelde yer tutucu, veritabanı bağlantısı veya güvenlik anahtarıyla ilgilidir. |
| "yer tutucu var" hatası | `web.config`'te büyük harfli bir değer doldurulmamış. |
| `Access denied for user` / `Unknown database` | Veritabanı adı, kullanıcı veya parola yanlış; Plesk'in eklediği önekleri kontrol edin. |
| `/health` → Unhealthy | MySQL çalışmıyor veya bağlantı bilgisi yanlış. |
| Sayfa yenileyince 404 | Dosyalar alt klasöre çıkmış; `API.dll` belge kökünde olmalı. |
| Duyuru/bildirim saatleri kaymış | Sunucunun Windows saat dilimi "(UTC+03:00) İstanbul" olmalı. |
| Hata ayrıntıları | `logs\api-YYYYMMDD.log`. Klasöre yazılamıyorsa Dosya Yöneticisi → `logs` → İzinleri değiştir → uygulama havuzu kullanıcısına yazma izni verin. |

Takıldığınız yerde hata mesajını veya log dosyasının son satırlarını gönderin.

## Güncelleme (yeni paket geldiğinde)

1. Belge köküne `app_offline.htm` adlı boş bir dosya oluşturun; site bakım moduna geçer.
2. Yeni zip'i yükleyip çıkarın. **`web.config`'in üzerine yazmayın**: zip'teki web.config'i atlayın veya önce
   kendi dosyanızın yedeğini alın. `logs` klasörü kalabilir.
3. `app_offline.htm`'yi silin. Veritabanı şeması varsa açılışta otomatik güncellenir.

## Veri ve yedek

- Test verileri MySQL veritabanındadır. Plesk → Veritabanları → **Dışa aktar** veya Yedekleme Yöneticisi ile
  yedek alabilirsiniz.
- Testi sıfırdan başlatmak için veritabanındaki tüm tabloları silip siteyi yeniden başlatmanız yeterli. Şema ve
  ilk yönetici tekrar oluşur; bunun için `Seed__AdminPassword` satırı web.config'te olmalı.
