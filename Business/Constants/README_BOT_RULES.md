# YasirSharp AI - Intelligent Bot Rules Engine

## 📋 Genel Bakış

**BotRules.cs** - Akıllı kural tabanlı soru-cevap motoru. Backend'de çalışan, kullanıcı sorularını analiz eden ve detaylı Markdown cevaplar üreten güçlü bir sistem.

---

## 🎯 Özellikler

### 1. **Intent Detection (Niyet Tespiti)**
Kullanıcının sorusundan **15+ farklı niyet** tespit eder:

- ✅ `add_course` - Ders ekleme
- ✅ `add_exam` - Sınav ekleme
- ✅ `view_calendar` - Takvim görüntüleme
- ✅ `manage_classroom` - Derslik yönetimi
- ✅ `manage_teacher` - Öğretim görevlisi yönetimi
- ✅ `settings` - Ayarlar/Profil
- ✅ `dashboard` - Dashboard/Ana sayfa
- ✅ `edit_exam` - Sınav düzenleme
- ✅ `delete_exam` - Sınav silme
- ✅ `export` - Excel/PDF export
- ✅ `import` - Toplu içe aktarma
- ✅ `filter` - Filtreleme/Arama
- ✅ `conflict_check` - Çakışma kontrolü
- ✅ `notification` - Bildirim yönetimi
- ✅ `help` - Genel yardım
- ✅ `general` - Bağlamsal genel cevap

### 2. **Markdown Formatted Answers**
Her intent için:
- 📝 Detaylı adım adım talimatlar
- 💡 Kullanım ipuçları
- ⚠️ Önemli uyarılar
- ✨ Emoji'lerle zenginleştirilmiş içerik

### 3. **Quick Actions (Hızlı Aksiyonlar)**
Her cevapla birlikte **tıklanabilir butonlar**:
- 🎯 Sayfa yönlendirmeleri
- 📁 Dosya indirme linkleri
- 🎬 Video rehber linkleri
- ⚡ Spotlight tour başlatma

### 4. **Page Guide References**
Her cevap ilgili **sayfa rehberine** referans verir:
- `examSchedule` - Takvim rehberi
- `courses` - Dersler rehberi
- `classrooms` - Derslikler rehberi
- `teachers` - Öğretim görevlileri rehberi
- `settings` - Ayarlar rehberi

---

## 🔧 Kullanım

### API Endpoint

```http
POST /api/assistant/ask
Content-Type: application/json
Authorization: Bearer {token}

{
  "userId": 1,
  "question": "yeni bir ders eklemek istiyorum",
  "pageContext": "dashboard",
  "userRole": "admin",
  "language": "tr"
}
```

### Response

```json
{
  "success": true,
  "message": "Bot yanıtı başarıyla oluşturuldu.",
  "data": {
    "answer": "📚 **Yeni Ders Eklemek İçin:**\n\n1. Sol menüden **'Dersler'** sekmesine gidin...",
    "suggestedActions": [
      {
        "id": "goto_courses",
        "label": "Dersler Sayfasına Git",
        "icon": "book-open",
        "path": "/courses",
        "description": "Ders ekleme sayfasını aç"
      }
    ],
    "pageGuideReference": "courses",
    "detectedIntent": "add_course"
  }
}
```

---

## 📊 Intent Examples

### Ders Ekleme
**Sorular:**
- "ders ekle"
- "yeni ders oluştur"
- "nasıl ders eklerim?"

**Cevap:**
- Adım adım ders ekleme talimatları
- "Dersler Sayfasına Git" butonu
- "Video Rehber İzle" butonu

---

### Sınav Ekleme
**Sorular:**
- "sınav ekle"
- "yeni sınav"
- "sınav nasıl eklenir?"

**Cevap:**
- 2 farklı yöntem (Takvim + Sınavlar sayfası)
- Sürükle-bırak özelliği açıklaması
- Çakışma kontrolü bilgisi
- "Sınav Takvimi" ve "Sınavlar Listesi" butonları

---

### Takvim Görüntüleme
**Sorular:**
- "takvim"
- "sınav takvimini göster"
- "takvime git"

**Cevap:**
- Takvim özellikleri (sürükle-bırak, görünüm değiştirme, filtre)
- Klavye kısayolları (T, →, ←, M, W)
- "Takvimi Aç" ve "İnteraktif Tur Başlat" butonları

---

### Çakışma Kontrolü
**Sorular:**
- "çakışma"
- "çakışan sınavlar"
- "aynı anda"

**Cevap:**
- Otomatik kontrol mekanizması
- 3 tip çakışma (derslik, öğretmen, bölüm)
- Çakışma durumunda yapılacaklar
- "Takvimi Kontrol Et" butonu

---

## 🎨 Contextual Answers

Eğer intent tespit edilemezse, **sayfa bağlamına göre** dinamik cevap üretir:

```csharp
// Dashboard'dayken:
"Dashboard'da geziniyorsunuz. Size nasıl yardımcı olabilirim?"

// Takvim sayfasındayken:
"Sınav Takvimi sayfasındasınız. Takvim özellikleri hakkında soru sorabilirsiniz."

// Dersler sayfasındayken:
"Dersler sayfasındasınız. Ders yönetimi hakkında yardımcı olabilirim."
```

Ayrıca **sayfa bazlı quick actions** önerir:
- Dashboard → "Yeni Sınav Ekle", "Takvimi Görüntüle"
- Takvim → "PDF İndir", "Filtre Uygula"
- Dersler → "Yeni Ders Ekle", "Toplu İçe Aktar"

---

## 🚀 Genişletme

### Yeni Intent Ekleme

1. **Anahtar kelimeleri tanımla:**
```csharp
private static readonly string[] YourFeatureKeywords = new[]
{
    "kelime1", "kelime2", "kelime3"
};
```

2. **Intent detection'a ekle:**
```csharp
if (ContainsAny(q, YourFeatureKeywords))
    return "your_feature";
```

3. **Cevap üret:**
```csharp
case "your_feature":
    response.Answer = "📌 **Your Feature:**\n\nDetaylı açıklama...";
    response.SuggestedActions.Add(new QuickActionDto { ... });
    response.PageGuideReference = "yourPage";
    break;
```

---

## 📝 Örnek Soru-Cevap Akışı

**Kullanıcı:** "yeni bir ders eklemek istiyorum"

1. ✅ **Intent Detection:** `add_course`
2. ✅ **Answer Generation:** Markdown formatında detaylı adımlar
3. ✅ **Quick Actions:** "Dersler Sayfasına Git" butonu
4. ✅ **Page Reference:** `courses`
5. ✅ **Auto-save:** Etkileşim database'e kaydedilir

**Bot Cevabı:**
```
📚 **Yeni Ders Eklemek İçin:**

1. Sol menüden **'Dersler'** sekmesine gidin
2. Sağ üstteki **'Yeni Ders Ekle'** butonuna tıklayın
3. Ders bilgilerini doldurun:
   - Ders Kodu (örn: BİL101)
   - Ders Adı
   - Kredi bilgisi
   - Bölüm seçimi
4. **'Kaydet'** butonuna tıklayın

✨ Kaydettikten sonra hemen sınav oluşturabilirsiniz!

[Dersler Sayfasına Git] [Video Rehber İzle]
```

---

## 🔥 Avantajlar

1. **Offline AI:** Backend'de çalışır, API key gerekmez
2. **Fast Response:** Milisaniyeler içinde cevap
3. **Customizable:** Her intent özelleştirilebilir
4. **Extensible:** Yeni intent'ler kolayca eklenebilir
5. **Multi-language Ready:** İngilizce cevaplar kolayca eklenebilir
6. **Analytics Friendly:** Tüm etkileşimler loglanır

---

## 📊 İstatistikler

- **15+ Intent** tespit kapasitesi
- **100+ Anahtar kelime** tanımlı
- **Markdown formatting** desteği
- **Quick actions** her cevapta
- **Page references** otomatik
- **Auto-logging** her etkileşimde

---

## 🎯 Gelecek Planlar

- [ ] İngilizce cevaplar ekle
- [ ] OpenAI entegrasyonu (opsiyonel)
- [ ] Intent confidence scoring
- [ ] Machine learning ile intent iyileştirme
- [ ] Kullanıcı feedback sistemi
- [ ] A/B testing için multiple answers

---

## 📞 İletişim

Sorularınız için: **YasirSharp AI Team** 🤖
