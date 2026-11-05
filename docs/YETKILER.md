# Roller ve yetkiler

Yetkiler **sunucuda** denetlenir. Arayüz yalnızca kullanıcının yapamayacağı düğmeleri gizler ya da kilitler.
Rol tanımları `Business/Constants/Roles.cs` dosyasındadır. Bölüm kapsamı `API/Security/BolumScope.cs` dosyasında
uygulanır.

| Rol | Kod adı | Kısaca |
|---|---|---|
| Süper yönetici | `super.admin` | Her şey; diğer süper yöneticileri de yönetir; **kullanıcı olarak girebilir** |
| Yönetici | `Admin` | Her şey; süper yönetici hesaplarına dokunamaz |
| Bölüm görevlisi | `bolum.gorevlisi` | **Yalnızca kendi bölüm(ler)inin** planlaması |
| Görevli personel | `gorevli.personel` | **Atandığı bölüm(ler)in** her şeyini görür, hiçbir şeyi değiştiremez |
| Akademik personel | `akademik.personel` | **Yalnızca** dersini verdiği ve gözetmen olduğu sınavlar, değişiklik talebi, kendi müsaitliği |
| Öğrenci | `ogrenci` | Kendi bölümünün sınav takvimi |

Bir kişide birden çok rol olabilir. Örneğin bir doçent hem akademik personel hem bölüm görevlisidir. Geniş olan
rolün kuralları geçerlidir: görevli olan bir hoca kendi bölümünün tamamını görür. Yönetici rolü olan bir kişi her
yerde sınırsızdır.

## Görevli personel ve akademik personelin kapsamı

- **Görevli personel** bir veya birden çok bölüme atanır: Kullanıcılar → Roller → "Görevli Personel" işaretliyken
  altta bölümler seçilir (`KullaniciBolum` tablosu, `GET/PUT api/User/{id}/okuma-bolumleri`).
  - O bölümlerin sınavlarını, derslerini, dersliklerini, ders atamalarını, öğrencilerini, raporlarını ve
    personelin müsaitliklerini görür.
  - Hiçbir şeyi değiştiremez.
  - Bölüm atanmamış görevli personel hiçbir bölüm verisi görmez.
  - Atama da her istekte veritabanından okunur.
- **Akademik personel** (görevli değilse) yalnızca kendi görevlerini görür:
  - Sınav takvimi ve raporlar: dersini verdiği ve gözetmen olduğu sınavlar.
  - Ders atamaları: yalnızca kendi dersleri.
  - Derslik adları: takvimde görünsün diye açık.
  - Kapalı olanlar: öğrenci listesi ve başkasının müsaitliği.

## Bölüm görevlisinin kapsamı

- Görevlinin bölümleri, **Bölümler → görevliler** atamasından gelir. Sunucu bu atamayı **her istekte
  veritabanından** okur. Görevlilikten alınan kişi, elindeki oturum süresi dolmasa da yetkisini hemen kaybeder.
- **Görür:**
  - Kendi bölümlerinin sınavları, ders atamaları ve öğrencileri.
  - Kendi bölümlerinin dersleri ve derslikleri, ayrıca ortak (bölümsüz) ders ve derslikler. Ortak kayıtlar sınav planlamak için gerekir.
  - Dersini verdiği ya da gözetmen olduğu başka bölüm sınavları. Bunları görür ama değiştiremez.
- **Değiştirir:**
  - Yalnızca **bütün bölümleri kendi bölümleri olan** kayıtları değiştirir.
  - Ortak kayıtları ve birden çok bölümü olup içlerinden biri başkasına ait kayıtları (ör. İnşaat ve Makine'de okutulan bir ders) yalnızca yönetici değiştirir.
- Gözetmeni her bölümden seçebilir; gözetmen havuzu ortaktır.
- Yetkisiz bir değişiklik `403` döndürür. Mesaj: "Sadece kendi bölümünüzün kayıtlarını görebilir ve
  değiştirebilirsiniz." ya da "Ortak veya birden çok bölüme ait kayıtları yalnızca yönetici değiştirebilir."

## İşlem matrisi

Kısaltmalar: **Y** = yönetici (super.admin, Admin), **BG** = bölüm görevlisi, **GP** = görevli personel,
**AP** = akademik personel, **Ö** = öğrenci.

| İşlem | Y | BG | GP | AP | Ö |
|---|---|---|---|---|---|
| Sınav takvimini görme | tümü | kendi bölümü + kendi görevleri | atandığı bölümler | yalnız kendi görevleri | kendi bölümü |
| Sınav ekleme / taşıma / silme | ✓ | kendi bölümü | – | – | – |
| Sınavda derslik ve gözetmen | ✓ | kendi bölümünün sınavında | – | – | – |
| Sınav değişiklik talebi açma | – | kendi görevleri | – | kendi görevleri | – |
| Talebi kabul / red | tümü | kendi bölümü | – | – | – |
| Resmi sınav takvimi PDF | tümü | kendi bölümü | atandığı bölümler | kendi görevleri | kendi bölümü |
| Gözetmen ve derslik raporu | tümü | kendi bölümü | atandığı bölümler | kendi görevleri | – |
| Dersler: görme | tümü | kendi bölümü + ortak | atandığı bölümler + ortak | – | – |
| Dersler: ekleme / değiştirme / silme | ✓ | yalnız kendi bölümüne ait | – | – | – |
| Derslikler: görme | tümü | kendi bölümü + ortak | atandığı bölümler + ortak | adlar (takvim için) | – |
| Derslikler: ekleme / değiştirme / silme | ✓ | yalnız kendi bölümüne ait | – | – | – |
| Ders atamaları: görme | tümü | kendi bölümü + verdiği dersler | atandığı bölümler | verdiği dersler | takvimde ders kodu için |
| Ders atamaları: ekleme / değiştirme / silme | ✓ | kendi bölümü | – | – | – |
| Bölüm ekleme, görevli atama | ✓ | – | – | – | – |
| Akademik personel ekleme / değiştirme | ✓ | – | – | – | – |
| Öğrenci listesi (kişisel veri) | tümü | kendi bölümü | atandığı bölümler | – | yalnız kendisi |
| Öğrenci ekleme / değiştirme / silme | ✓ | – | – | – | – |
| Müsaitlik görme | tümü | tümü | tümü | yalnız kendisinin | – |
| Müsaitlik değiştirme | herkesin | yalnız kendisinin | yalnız kendisinin | yalnız kendisinin | – |
| Duyuru yayınlama | herkese | yalnız kendi bölümüne | – | – | – |
| Kullanıcılar, roller, parola sıfırlama | ✓ (süper yönetici hariç) | – | – | – | – |
| Elle bildirim gönderme (`POST api/Notification`) | ✓ | – | – | – | – |
| Ham sınav-derslik kayıtları (`api/SinavDerslik`) | ✓ | – | – | – | – |
| Hata bildirimi gönderme | ✓ | ✓ | ✓ | ✓ | ✓ |
| Hata bildirimlerini görme / yönetme | ✓ | – | – | – | – |

Notlar:
- `api/SinavDerslik` ham kayıt uçları çakışma ve müsaitlik kontrolü yapmaz. Arayüz sınav dersliklerini
  `api/SinavDetay` üzerinden, kontrollü olarak değiştirir.
- Başka bölümün sınavıyla çakışma, kaydetme sırasında sunucu tarafından yine yakalanır; kapsam yalnızca
  görmeyi sınırlar.
- Talep kabul edilince sınav kendiliğinden değişmez. Görevli sınavı takvimde düzenler; çakışma kontrolü her
  zamanki gibi çalışır. Talep açılınca o bölümün görevlilerine ve yöneticilere, yanıtlanınca talep edene bildirim
  gider (`api/SinavTalep`).
- Otomasyon uçları (`api/automation/bug-reports`) kullanıcı rolüyle değil, otomasyon anahtarıyla açılır. Yalnızca
  hata bildirimlerini okur ve not yazar. Bkz. `PLESK_KURULUM.md`.

## Kullanıcı olarak girme (yalnızca süper yönetici)

- `POST api/Auth/impersonate/{userId}`: hedef kişinin **kendi rolleri ve bölümleriyle** normal bir token döner.
  Sistem o kişi girmiş gibi davranır. Token'da `act_as_by` (süper yöneticinin Id'si) ve `act_as_by_name` vardır.
- `POST api/Auth/stop-impersonation`: `act_as_by` sahibine yeni bir token üretir. Süper yöneticinin asıl token'ı
  tarayıcıda saklanmaz.
- Kişi olarak girilmişken başka bir kişiye doğrudan geçilebilir; sunucu gerçek kişinin süper yönetici olduğunu
  denetler.
- Yasaklar:
  - Süper yönetici olmayan biri kimseye geçemez.
  - Kendine, pasif hesaba ve başka bir süper yöneticiye geçilemez.
  - Kişi olarak girilmişken parola değiştirilemez.
  - Kurallar `API/Security/ImpersonationRules.cs` dosyasındadır.
- Denetim:
  - Her geçiş ve kişi olarak yapılan her değiştirme isteği sunucu loguna yazılır: "Kişi olarak işlem: X → Y".
  - Hata bildirimlerinde ad "Y (X adına)" olarak görünür.
- Arayüz:
  - Kullanıcılar ve Akademik Personel satırında **⋯ → Bu kullanıcı olarak gir**.
  - Üstte turuncu bir şerit görünür.
  - Hesap menüsünde **Kullanıcı değiştir** bulunur: kendi hesabına dönüş ve son 5 kişi. Bu liste yalnızca o tarayıcıda tutulur.

## Testler

- Kurallar: `Tests/BolumScopeTests.cs`, `Tests/ImpersonationTests.cs`.
- Uçtan uca: arayüz reposunda `e2e/10-roller-ve-gecis.spec.ts`. Kapsam:
  - Akademik personelin kapsamı
  - Değişiklik talebi ve kabulü
  - Kullanıcı olarak girip geri dönme ve hızlı geçiş
  - Görevli personelin çoklu bölümü
  - Role göre menü
- Uçtan uca: arayüz reposunda `e2e/09-bolum-izolasyonu.spec.ts`. Kapsam:
  - Başka bölümün sınavı, dersi, dersliği ve ataması
  - Ortak kayıtlar
  - Ham uçlar
  - Müsaitlik
  - Öğrenci listesi
  - Arayüzdeki kilitler
  - Görevlilikten çıkarılınca token'ın geçersizleşmesi
