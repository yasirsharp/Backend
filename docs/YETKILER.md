# Roller ve yetkiler

Yetkiler **sunucuda** denetlenir. Arayüz yalnızca kullanıcının yapamayacağı düğmeleri gizler ya da kilitler.
Rol tanımları `Business/Constants/Roles.cs` dosyasındadır. Bölüm kapsamı `API/Security/BolumScope.cs` dosyasında
uygulanır.

| Rol | Kod adı | Kısaca |
|---|---|---|
| Süper yönetici | `super.admin` | Her şey; diğer süper yöneticileri de yönetir |
| Yönetici | `Admin` | Her şey; süper yönetici hesaplarına dokunamaz |
| Bölüm görevlisi | `bolum.gorevlisi` | **Yalnızca kendi bölüm(ler)inin** planlaması |
| Görevli personel | `gorevli.personel` | Tanımları ve müsaitlikleri görür |
| Akademik personel | `akademik.personel` | Kendi sınav görevleri ve kendi müsaitliği |
| Öğrenci | `ogrenci` | Kendi bölümünün sınav takvimi |

Bir kişide birden çok rol olabilir. Örneğin bir doçent hem akademik personel hem bölüm görevlisidir.
Yönetici rolü olan bir kişi her yerde sınırsızdır.

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
| Sınav takvimini görme | tümü | kendi bölümü + kendi görevleri | tümü | tümü | kendi bölümü |
| Sınav ekleme / taşıma / silme | ✓ | kendi bölümü | – | – | – |
| Sınavda derslik ve gözetmen | ✓ | kendi bölümünün sınavında | – | – | – |
| Resmi sınav takvimi PDF | tümü | kendi bölümü | tümü | tümü | kendi bölümü |
| Gözetmen ve derslik raporu | tümü | kendi bölümü | tümü | tümü | – |
| Dersler: görme | tümü | kendi bölümü + ortak | tümü | tümü | – |
| Dersler: ekleme / değiştirme / silme | ✓ | yalnız kendi bölümüne ait | – | – | – |
| Derslikler: görme | tümü | kendi bölümü + ortak | tümü | tümü | – |
| Derslikler: ekleme / değiştirme / silme | ✓ | yalnız kendi bölümüne ait | – | – | – |
| Ders atamaları: görme | tümü | kendi bölümü + verdiği dersler | tümü | tümü | takvimde ders kodu için |
| Ders atamaları: ekleme / değiştirme / silme | ✓ | kendi bölümü | – | – | – |
| Bölüm ekleme, görevli atama | ✓ | – | – | – | – |
| Akademik personel ekleme / değiştirme | ✓ | – | – | – | – |
| Öğrenci listesi (kişisel veri) | tümü | kendi bölümü | – | – | yalnız kendisi |
| Öğrenci ekleme / değiştirme / silme | ✓ | – | – | – | – |
| Müsaitlik görme | tümü | tümü | tümü | tümü | – |
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
- Görevli ve akademik personelin takvimi tüm bölümleriyle görmesi kasıtlıdır; yazma yetkileri yoktur. Bölüm
  görevlisinin görmesi ise istenerek kendi bölümüyle sınırlandı. Başka bölümün sınavıyla çakışma, kaydetme
  sırasında sunucu tarafından yine yakalanır.
- Otomasyon uçları (`api/automation/bug-reports`) kullanıcı rolüyle değil, otomasyon anahtarıyla açılır. Yalnızca
  hata bildirimlerini okur ve not yazar. Bkz. `PLESK_KURULUM.md`.

## Testler

- Kurallar: `Tests/BolumScopeTests.cs`.
- Uçtan uca: arayüz reposunda `e2e/09-bolum-izolasyonu.spec.ts`. Kapsam:
  - Başka bölümün sınavı, dersi, dersliği ve ataması
  - Ortak kayıtlar
  - Ham uçlar
  - Müsaitlik
  - Öğrenci listesi
  - Arayüzdeki kilitler
  - Görevlilikten çıkarılınca token'ın geçersizleşmesi
