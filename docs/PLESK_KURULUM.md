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

## Güncelleme (yeni paket geldiğinde, elle)

Otomatik yol için aşağıdaki "Otomatik güncelleme" bölümüne bakın.


1. Belge köküne `app_offline.htm` adlı boş bir dosya oluşturun; site bakım moduna geçer.
2. Yeni zip'i yükleyip çıkarın. **`web.config`'in üzerine yazmayın**: zip'teki web.config'i atlayın veya önce
   kendi dosyanızın yedeğini alın. `logs` klasörü kalabilir.
3. `app_offline.htm`'yi silin. Veritabanı şeması varsa açılışta otomatik güncellenir.

## Otomatik güncelleme (GitHub Actions)

Elle zip yüklemek yerine yeni sürüm GitHub'dan tek tıkla gönderilebilir. **Test sitesine yükle** workflow'u
(`.github/workflows/deploy-test.yml`) şunları yapar:

1. Arayüzü ve API'yi derler, birim testlerini ve denetimleri çalıştırır. Bir test kırılırsa hiçbir şey yüklenmez.
2. Siteyi bakım moduna alır (`app_offline.htm`; ziyaretçi "Sistem güncelleniyor" sayfası görür).
3. FTP ile **yalnızca değişen dosyaları** yükler, artık kullanılmayan eski dosyaları siler.
   - **`web.config` ve `logs` klasörüne asla dokunmaz.** Veritabanı parolası ve güvenlik anahtarı sunucuda
     kalır.
   - Fark bilgisi sunucuda `.ftp-deploy-sync-state.json` dosyasında tutulur; bu dosyayı silmeyin. Silinirse
     sonraki yükleme her şeyi baştan gönderir, başka bir zararı yoktur.
4. Bakım modunu kaldırır. Yükleme yarıda kalsa bile kaldırır.
5. `/health` adresinin "Healthy" döndüğünü ve yayındaki arayüzün yeni derleme olduğunu kontrol eder.

### Bir kez yapılacaklar

**a) Plesk'te FTP hesabı**

1. **Web Siteleri ve Alan Adları** → `hafiz.yasirsharp.com.tr` → **FTP Erişimi** → **FTP Hesabı Ekle**.
2. Kullanıcı adı ve güçlü bir parola belirleyin (ör. `hafiz-deploy`).
3. **Ana dizin** olarak subdomain'in belge kökünü seçin; `API.dll` ve `web.config`'in bulunduğu klasör.
4. FTP sunucu adresi genelde ana alan adı (`yasirsharp.com.tr`) veya sunucunun IP adresidir.
5. Tercihen FTPS (FTP üzerinden TLS) açık olsun: Araçlar ve Ayarlar → **FTP Ayarları** (Plesk sürümüne göre
   "SSL/TLS" seçeneği). Güvenlik duvarında 21 ve pasif FTP portları açık olmalıdır.

Bir FTP istemcisiyle (ör. FileZilla) bağlanıp `web.config`'i gördüğünüzü doğrulayın.

**b) GitHub'da sırlar**

Backend reposunda: **Settings** → **Secrets and variables** → **Actions** → **New repository secret**.

| Ad | Değer |
|---|---|
| `FTP_SERVER` | FTP sunucu adresi, yalnızca ad veya IP (ör. `yasirsharp.com.tr`); port 21 kullanılır |
| `FTP_USERNAME` | (a)'daki FTP kullanıcı adı |
| `FTP_PASSWORD` | (a)'daki parola |
| `FTP_REMOTE_DIR` | İsteğe bağlı. FTP ana dizini belge kökü değilse, ana dizine göre klasör (ör. `hafiz.yasirsharp.com.tr`). Boş bırakın. |

Sırlar loglarda gizlenir; repoya veya sohbete yazmayın.

**c) İsteğe bağlı ikinci onay**

**Settings** → **Environments** → `test-hafiz` (ilk çalıştırmada kendiliğinden oluşur) → **Required reviewers**
ile kendinizi ekleyin. Her yükleme bir de "Approve" ekranında onay bekler.

**d) Workflow'un görünmesi**

GitHub, elle çalıştırılan workflow'ların **Run workflow** düğmesini dosya reponun varsayılan dalında (`main`)
bulunduğunda gösterir. Uygulama kodu main'e birleştirilmeden yalnızca bu tek dosyanın main'e eklenmesi yeter;
çalıştırırken dal olarak test dalı seçilir ve o dalın kodu yüklenir.

### Her güncellemede

1. GitHub → **Actions** → **Test sitesine yükle** → **Run workflow**.
2. **Use workflow from:** `claude/eager-brahmagupta-f1g0id` (backend dalı).
3. **Onay:** `hafiz` yazın. **Arayüz dalı** varsayılan kalabilir.
4. Yaklaşık 8–12 dakika sürer. Bitince özet sayfasında backend ve arayüz commit'leri ile sürüm yazar. Sitenin
   alt bilgisindeki sürüm de (ör. `v0.10.0 (f0cf193)`) yeni olmalıdır.

İlk çalıştırmada fark bilgisi olmadığı için tüm paket (~80 MB) gönderilir; sonrakiler genelde birkaç MB'tır.

### Sorun giderme

| Belirti | Çözüm |
|---|---|
| "Eksik GitHub secret" | (b)'deki sırları ekleyin; adlar birebir aynı olmalı. |
| `530 Login incorrect` / `Login authentication failed` | FTP kullanıcı adı veya parola yanlış. Plesk'te FTP hesabının parolasını yenileyip sırrı güncelleyin. |
| `certificate`/`self signed`/`unable to verify` | Sunucunun FTP sertifikası doğrulanamıyor. Çalıştırırken **FTPS sertifikasını doğrula** seçeneğini kapatın. |
| `SSL connection` / `AUTH TLS` reddedildi | Sunucuda FTPS kapalı. Plesk'te açın veya **ftp_protokol** olarak `ftp` seçin (parola şifresiz gider). |
| Bağlantı zaman aşımı / dizin listelenemiyor | Güvenlik duvarı pasif FTP portlarını engelliyor. Windows Güvenlik Duvarı'nda IIS FTP pasif port aralığını açın. |
| `550` izin hatası | FTP hesabının ana dizinine yazma izni yok veya `FTP_REMOTE_DIR` yanlış. |
| Sağlık kontrolü başarısız | Yükleme tamam ama uygulama açılmadı. `logs\api-*.log` dosyasına bakın; site bakım modunda kalmaz. |
| "Yayındaki index.html yeni derlemeyi göstermiyor" | Tarayıcı/proxy önbelleği veya yükleme eksik. Workflow'u tekrar çalıştırın. |
| Site "Sistem güncelleniyor" sayfasında kaldı | Dosya Yöneticisi'nden belge kökündeki `app_offline.htm` dosyasını silin. |

## Veri ve yedek

- Test verileri MySQL veritabanındadır. Plesk → Veritabanları → **Dışa aktar** veya Yedekleme Yöneticisi ile
  yedek alabilirsiniz.
- Testi sıfırdan başlatmak için veritabanındaki tüm tabloları silip siteyi yeniden başlatmanız yeterli. Şema ve
  ilk yönetici tekrar oluşur; bunun için `Seed__AdminPassword` satırı web.config'te olmalı.
