using Entity.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Business.Constants
{
    /// <summary>
    /// YasirSharp AI - Intelligent Bot Rules Engine
    /// Akıllı kural tabanlı soru-cevap motoru
    /// 26 Ekim 2025
    /// </summary>
    public static class BotRules
    {
        #region Intent Patterns (Anahtar Kelimeler)

        /// <summary>
        /// Ders ekleme ile ilgili anahtar kelimeler
        /// </summary>
        private static readonly string[] AddCourseKeywords = new[]
        {
            "ders ekle", "yeni ders", "ders oluştur", "ders eklemek",
            "nasıl ders eklerim", "ders nasıl eklenir", "ders kaydet",
            "course add", "add course", "new course"
        };

        /// <summary>
        /// Sınav ekleme ile ilgili anahtar kelimeler
        /// </summary>
        private static readonly string[] AddExamKeywords = new[]
        {
            "sınav ekle", "yeni sınav", "sınav oluştur", "sınav eklemek",
            "nasıl sınav eklerim", "sınav nasıl eklenir", "sınav kaydet",
            "exam add", "add exam", "new exam", "imtihan"
        };

        /// <summary>
        /// Takvim ile ilgili anahtar kelimeler
        /// </summary>
        private static readonly string[] CalendarKeywords = new[]
        {
            "takvim", "sınav takvimi", "takvime git", "takvimi göster",
            "takvim görünümü", "calendar", "schedule", "zamanlama"
        };

        /// <summary>
        /// Derslik ile ilgili anahtar kelimeler
        /// </summary>
        private static readonly string[] ClassroomKeywords = new[]
        {
            "derslik", "derslik ekle", "yeni derslik", "sınıf",
            "classroom", "room", "oda ekle"
        };

        /// <summary>
        /// Öğretim görevlisi ile ilgili anahtar kelimeler
        /// </summary>
        private static readonly string[] TeacherKeywords = new[]
        {
            "öğretmen", "hoca", "öğretim görevlisi", "görevli",
            "teacher", "instructor", "hocam", "öğretim üyesi"
        };

        /// <summary>
        /// Profil/Ayarlar ile ilgili anahtar kelimeler
        /// </summary>
        private static readonly string[] SettingsKeywords = new[]
        {
            "ayar", "ayarlar", "profil", "şifre değiştir",
            "settings", "profile", "hesap", "hesabım"
        };

        /// <summary>
        /// Dashboard ile ilgili anahtar kelimeler
        /// </summary>
        private static readonly string[] DashboardKeywords = new[]
        {
            "ana sayfa", "dashboard", "anasayfa", "ana ekran",
            "home", "main page", "başlangıç"
        };

        /// <summary>
        /// Yardım/Rehber ile ilgili anahtar kelimeler
        /// </summary>
        private static readonly string[] HelpKeywords = new[]
        {
            "yardım", "nasıl", "ne yapmalı", "anlamadım",
            "help", "guide", "rehber", "öğren", "öğret"
        };

        /// <summary>
        /// Sınav düzenleme ile ilgili anahtar kelimeler
        /// </summary>
        private static readonly string[] EditExamKeywords = new[]
        {
            "sınav düzenle", "sınavı değiştir", "sınav güncelle",
            "edit exam", "update exam", "modify exam", "sınav tarihini değiştir"
        };

        /// <summary>
        /// Sınav silme ile ilgili anahtar kelimeler
        /// </summary>
        private static readonly string[] DeleteExamKeywords = new[]
        {
            "sınav sil", "sınavı kaldır", "sınavı iptal et",
            "delete exam", "remove exam", "cancel exam"
        };

        /// <summary>
        /// Excel/Export ile ilgili anahtar kelimeler
        /// </summary>
        private static readonly string[] ExportKeywords = new[]
        {
            "excel", "dışa aktar", "export", "indir", "download",
            "pdf", "dosya indir", "yazdır", "print"
        };

        /// <summary>
        /// Import ile ilgili anahtar kelimeler
        /// </summary>
        private static readonly string[] ImportKeywords = new[]
        {
            "toplu", "içe aktar", "import", "yükle", "upload",
            "excel yükle", "dosyadan yükle", "çoklu ekleme"
        };

        /// <summary>
        /// Filtreleme ile ilgili anahtar kelimeler
        /// </summary>
        private static readonly string[] FilterKeywords = new[]
        {
            "filtre", "ara", "bul", "search", "filter",
            "göster", "listele", "sırala", "sort"
        };

        /// <summary>
        /// Çakışma kontrolü ile ilgili anahtar kelimeler
        /// </summary>
        private static readonly string[] ConflictKeywords = new[]
        {
            "çakışma", "çakışan", "conflict", "aynı anda",
            "aynı saatte", "çakışıyor", "üst üste"
        };

        /// <summary>
        /// Bildirim ile ilgili anahtar kelimeler
        /// </summary>
        private static readonly string[] NotificationKeywords = new[]
        {
            "bildirim", "notification", "duyuru", "announcement",
            "uyarı", "alert", "hatırlatma", "reminder"
        };

        #endregion

        #region Intent Detection (Niyet Tespiti)

        /// <summary>
        /// Kullanıcının sorusundan niyeti tespit et
        /// </summary>
        public static string DetectIntent(string question)
        {
            var q = question.ToLowerInvariant().Trim();

            // Ders ekleme niyeti
            if (ContainsAny(q, AddCourseKeywords))
                return "add_course";

            // Sınav ekleme niyeti
            if (ContainsAny(q, AddExamKeywords))
                return "add_exam";

            // Takvim görüntüleme
            if (ContainsAny(q, CalendarKeywords))
                return "view_calendar";

            // Derslik yönetimi
            if (ContainsAny(q, ClassroomKeywords))
                return "manage_classroom";

            // Öğretim görevlisi
            if (ContainsAny(q, TeacherKeywords))
                return "manage_teacher";

            // Ayarlar/Profil
            if (ContainsAny(q, SettingsKeywords))
                return "settings";

            // Dashboard
            if (ContainsAny(q, DashboardKeywords))
                return "dashboard";

            // Sınav düzenleme
            if (ContainsAny(q, EditExamKeywords))
                return "edit_exam";

            // Sınav silme
            if (ContainsAny(q, DeleteExamKeywords))
                return "delete_exam";

            // Excel/Export
            if (ContainsAny(q, ExportKeywords))
                return "export";

            // Import
            if (ContainsAny(q, ImportKeywords))
                return "import";

            // Filtreleme
            if (ContainsAny(q, FilterKeywords))
                return "filter";

            // Çakışma kontrolü
            if (ContainsAny(q, ConflictKeywords))
                return "conflict_check";

            // Bildirim
            if (ContainsAny(q, NotificationKeywords))
                return "notification";

            // Yardım
            if (ContainsAny(q, HelpKeywords))
                return "help";

            // Varsayılan
            return "general";
        }

        #endregion

        #region Answer Generation (Cevap Üretimi)

        /// <summary>
        /// Tespit edilen niyete göre akıllı cevap üret
        /// </summary>
        public static BotResponseDto GenerateAnswer(string intent, string question, string pageContext, string userRole)
        {
            var response = new BotResponseDto
            {
                Answer = "",
                SuggestedActions = new List<QuickActionDto>(),
                PageGuideReference = null
            };

            switch (intent)
            {
                case "add_course":
                    response.Answer = "📚 **Yeni Ders Eklemek İçin:**\n\n" +
                        "1. Sol menüden **'Akademik' > 'Dersler'** sekmesine gidin\n" +
                        "2. Sağ üstteki **'Yeni Ders Ekle'** butonuna tıklayın\n" +
                        "3. Ders bilgilerini doldurun:\n" +
                        "   - Ders Kodu (örn: BİL101)\n" +
                        "   - Ders Adı\n" +
                        "   - Kredi bilgisi\n" +
                        "   - Bölüm seçimi\n" +
                        "4. **'Kaydet'** butonuna tıklayın\n\n" +
                        "✨ Kaydettikten sonra hemen sınav oluşturabilirsiniz!";
                    
                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_courses",
                        Label = "Dersler Sayfasına Git",
                        Icon = "book-open",
                        Path = "/academic/dersler",
                        Description = "Ders ekleme sayfasını aç"
                    });

                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_dashboard",
                        Label = "Dashboard'a Dön",
                        Icon = "home",
                        Path = "/dashboard",
                        Description = "Ana sayfaya git"
                    });

                    response.PageGuideReference = "courses";
                    break;

                case "add_exam":
                    response.Answer = "📝 **Yeni Sınav Eklemek İçin İki Yöntem:**\n\n" +
                        "**📅 Yöntem 1: Takvimden (Önerilen)**\n" +
                        "1. **'Takvim'** sayfasına gidin\n" +
                        "2. Sol tarafta ders listesini görün\n" +
                        "3. Dersi takvime **sürükleyip bırakın** 🖱️\n" +
                        "4. Açılan formda detayları doldurun\n\n" +
                        "**📋 Yöntem 2: Manuel Ekleme**\n" +
                        "1. Takvim sayfasında **'Yeni Sınav Ekle'** butonuna tıklayın\n" +
                        "2. Formu doldurun:\n" +
                        "   - Ders seçimi\n" +
                        "   - Tarih ve saat\n" +
                        "   - Derslik\n" +
                        "   - Gözetmen öğretmen\n" +
                        "3. **'Kaydet'** butonuna tıklayın\n\n" +
                        "⚠️ Sistem otomatik olarak **çakışma kontrolü** yapar!";

                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_calendar",
                        Label = "Sınav Takvimi (Tavsiye Edilen)",
                        Icon = "calendar",
                        Path = "/calendar",
                        Description = "Sürükle-bırak ile sınav ekle"
                    });

                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_dashboard",
                        Label = "Dashboard'a Dön",
                        Icon = "home",
                        Path = "/dashboard",
                        Description = "Ana sayfaya git"
                    });

                    response.PageGuideReference = "examSchedule";
                    break;

                case "view_calendar":
                    response.Answer = "📅 **Sınav Takvimi - Güçlü Özellikler:**\n\n" +
                        "✨ **Kullanım İpuçları:**\n" +
                        "- **Sürükle-Bırak:** Sol taraftaki dersleri takvime sürükleyin\n" +
                        "- **Tarih Değiştirme:** Takvimde sınavları sürükleyerek taşıyın\n" +
                        "- **Görünüm:** Aylık/Haftalık/Günlük görünümleri deneyin\n" +
                        "- **Filtre:** Sadece belirli dersleri göstermek için filtre kullanın\n" +
                        "- **Çakışma Kontrolü:** Sistem otomatik kontrol eder\n\n" +
                        "🎯 **Hızlı Erişim:**\n" +
                        "Takvim menüsünden direkt erişebilirsiniz!";

                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_calendar",
                        Label = "Takvimi Aç",
                        Icon = "calendar",
                        Path = "/calendar",
                        Description = "Sınav takvimini görüntüle"
                    });

                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_dashboard",
                        Label = "Dashboard'a Dön",
                        Icon = "home",
                        Path = "/dashboard",
                        Description = "Ana sayfa"
                    });

                    response.PageGuideReference = "examSchedule";
                    break;

                case "manage_classroom":
                    response.Answer = "🏛️ **Derslik Yönetimi:**\n\n" +
                        "**Yeni Derslik Eklemek:**\n" +
                        "1. **'Akademik' > 'Derslikler'** sayfasına gidin\n" +
                        "2. **'Yeni Derslik Ekle'** butonuna tıklayın\n" +
                        "3. Bilgileri doldurun:\n" +
                        "   - Derslik Adı (örn: A Blok 101)\n" +
                        "   - Kapasite (öğrenci sayısı)\n" +
                        "   - Bina/Kat bilgisi\n" +
                        "   - Aktif/Pasif durumu\n\n" +
                        "💡 **İpuçları:**\n" +
                        "- Derslik kapasitesi sınav planlama için önemlidir\n" +
                        "- Bakım/tadilat sırasında derslikleri pasif yapabilirsiniz";

                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_classrooms",
                        Label = "Derslikler Sayfası",
                        Icon = "building",
                        Path = "/academic/derslikler",
                        Description = "Derslik yönetimine git"
                    });

                    response.PageGuideReference = "classrooms";
                    break;

                case "manage_teacher":
                    response.Answer = "👨‍🏫 **Akademik Personel Yönetimi:**\n\n" +
                        "**Yeni Personel Eklemek:**\n" +
                        "1. **'Akademik' > 'Personel'** sayfasına gidin\n" +
                        "2. **'Yeni Personel Ekle'** butonuna tıklayın\n" +
                        "3. Bilgileri doldurun:\n" +
                        "   - Ad Soyad\n" +
                        "   - Unvan (Prof., Doç., Dr. vb.)\n" +
                        "   - E-posta\n" +
                        "   - Telefon (opsiyonel)\n" +
                        "   - Bölüm\n\n" +
                        "📧 **Bildirim Sistemi:**\n" +
                        "Görevli oldukları sınavlar için otomatik bildirim gönderilir.";

                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_teachers",
                        Label = "Akademik Personel",
                        Icon = "users",
                        Path = "/academic/personel",
                        Description = "Personel yönetimine git"
                    });

                    response.PageGuideReference = "teachers";
                    break;

                case "settings":
                    response.Answer = "⚙️ **Ayarlar ve Profil:**\n\n" +
                        "**Yapabilecekleriniz:**\n" +
                        "- 👤 **Profil Bilgileri:** Ad, soyad, e-posta güncelleme\n" +
                        "- 🔒 **Güvenlik:** Şifre değiştirme\n" +
                        "- 🤖 **YasirSharp AI:** Bot tercihlerini ayarlama\n" +
                        "- 🌐 **Dil:** Türkçe/İngilizce seçimi\n" +
                        "- 🔔 **Bildirimler:** E-posta ve push bildirimleri\n\n" +
                        "🤖 **YasirSharp AI Ayarları:**\n" +
                        "- Botu tamamen kapatma/açma\n" +
                        "- Tercih edilen dil seçimi";

                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_dashboard",
                        Label = "Dashboard'a Dön",
                        Icon = "home",
                        Path = "/dashboard",
                        Description = "Ana sayfaya git"
                    });

                    response.PageGuideReference = "settings";
                    break;

                case "edit_exam":
                    response.Answer = "✏️ **Sınav Düzenleme:**\n\n" +
                        "**Takvimden Düzenleme:**\n" +
                        "1. **'/calendar'** sayfasına gidin\n" +
                        "2. Takvimde sınava **tıklayın**\n" +
                        "3. Açılan popup'ta **'Düzenle'** butonuna tıklayın\n" +
                        "4. Değişiklikleri yapın ve **'Kaydet'**\n\n" +
                        "⚠️ **Önemli:**\n" +
                        "- Tarih değiştirirken çakışma kontrolü yapılır\n" +
                        "- İlgili görevlilere otomatik bildirim gider";

                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_calendar",
                        Label = "Takvim Görünümü",
                        Icon = "calendar",
                        Path = "/calendar",
                        Description = "Takvimden düzenle"
                    });
                    break;

                case "delete_exam":
                    response.Answer = "🗑️ **Sınav Silme:**\n\n" +
                        "**Dikkat:** Silme işlemi geri alınamaz!\n\n" +
                        "**Takvimden Silme:**\n" +
                        "1. **'/calendar'** sayfasına gidin\n" +
                        "2. Silinecek sınava tıklayın\n" +
                        "3. Açılan popup'ta **'Sil'** butonuna tıklayın\n" +
                        "4. Onay penceresinde **'Evet, Sil'** deyin\n\n" +
                        "💡 **Alternatif:** Silmek yerine sınavı **pasif** hale getirebilirsiniz.";

                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_calendar",
                        Label = "Takvim Sayfası",
                        Icon = "calendar",
                        Path = "/calendar",
                        Description = "Sınav yönetimi"
                    });
                    break;

                case "export":
                    response.Answer = "📤 **Dışa Aktarma (Export):**\n\n" +
                        "**Takvim Export:**\n" +
                        "1. **'/calendar'** sayfasına gidin\n" +
                        "2. Sağ üstte **export butonlarını** bulun\n" +
                        "3. İstediğiniz formatı seçin (Excel/PDF)\n" +
                        "4. Dosya otomatik olarak indirilir\n\n" +
                        "📊 **Export İçeriği:**\n" +
                        "- Tüm sınav detayları\n" +
                        "- Tarih, saat, derslik bilgileri\n" +
                        "- Gözetmen öğretmenler\n" +
                        "- Filtrelenmiş veriler (filtre aktifse)";

                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_calendar",
                        Label = "Takvim Sayfası",
                        Icon = "calendar",
                        Path = "/calendar",
                        Description = "Export işlemleri"
                    });
                    break;

                case "import":
                    response.Answer = "📥 **Toplu İçe Aktarma (Import):**\n\n" +
                        "**Excel'den Toplu Sınav Ekleme:**\n" +
                        "Şu anda bu özellik geliştirme aşamasındadır.\n\n" +
                        "**Alternatif Yöntem:**\n" +
                        "Takvim sayfasından sürükle-bırak ile hızlıca sınav ekleyebilirsiniz.";

                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_calendar",
                        Label = "Takvime Git",
                        Icon = "calendar",
                        Path = "/calendar",
                        Description = "Sürükle-bırak ile ekle"
                    });
                    break;

                case "filter":
                    response.Answer = "🔍 **Filtreleme ve Arama:**\n\n" +
                        "**Takvim Filtreleme:**\n" +
                        "- Takvim sayfasında **filtre butonlarını** kullanın\n" +
                        "- Derslere göre filtreleyebilirsiniz\n" +
                        "- Tarih aralığı seçebilirsiniz\n\n" +
                        "**Diğer Sayfalarda:**\n" +
                        "- Her listede arama kutusu bulunur\n" +
                        "- Ders kodu/adı ile arama yapabilirsiniz";

                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_calendar",
                        Label = "Takvim Sayfası",
                        Icon = "calendar",
                        Path = "/calendar",
                        Description = "Filtreleme yap"
                    });
                    break;

                case "conflict_check":
                    response.Answer = "⚠️ **Çakışma Kontrolü:**\n\n" +
                        "**Otomatik Kontroller:**\n" +
                        "Sistem her sınav eklendiğinde/güncellendiğinde otomatik olarak kontrol eder:\n\n" +
                        "✅ **Kontrol Edilen Durumlar:**\n" +
                        "1. **Derslik Çakışması:** Aynı derslik, aynı saat\n" +
                        "2. **Öğretmen Çakışması:** Aynı görevli, aynı saat\n" +
                        "3. **Bölüm Çakışması:** Aynı bölüm, aynı saat\n\n" +
                        "🚨 **Çakışma Bulunursa:**\n" +
                        "- Kırmızı uyarı mesajı gösterilir\n" +
                        "- Çakışan sınav detayları listelenir\n\n" +
                        "💡 **Manuel Kontrol:**\n" +
                        "Takvim görünümünde çakışan sınavları görebilirsiniz.";

                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_calendar",
                        Label = "Takvimi Kontrol Et",
                        Icon = "alert-triangle",
                        Path = "/calendar",
                        Description = "Çakışmaları görsel olarak gör"
                    });
                    break;

                case "notification":
                    response.Answer = "🔔 **Bildirimler:**\n\n" +
                        "**Bildirim Türleri:**\n" +
                        "- 📧 **E-posta Bildirimleri:** Önemli güncellemeler\n" +
                        "- 🔔 **Sistem Duyuruları:** Genel duyurular\n\n" +
                        "**Duyurular Sayfası:**\n" +
                        "Sağ üstte **bildirim ikonu**na tıklayarak duyuruları görebilirsiniz.\n\n" +
                        "**Bildirim Alacağınız Durumlar:**\n" +
                        "- Yeni duyurular\n" +
                        "- Sistem güncellemeleri\n" +
                        "- Önemli hatırlatmalar";

                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_announcements",
                        Label = "Duyurular",
                        Icon = "bell",
                        Path = "/announcements",
                        Description = "Duyuruları görüntüle"
                    });
                    break;

                case "dashboard":
                    response.Answer = "🏠 **Dashboard - Ana Sayfa:**\n\n" +
                        "Dashboard'da görebilecekleriniz:\n\n" +
                        "📊 **İstatistikler:**\n" +
                        "- Toplam sınav sayısı\n" +
                        "- Yaklaşan sınavlar (7 gün)\n" +
                        "- Aktif ders sayısı\n" +
                        "- Derslik doluluk oranı\n\n" +
                        "📅 **Bugünün Ajandası:**\n" +
                        "- Bugünkü sınavlar\n" +
                        "- Gözetmenlik görevleri\n" +
                        "- Bekleyen işlemler\n\n" +
                        "⚡ **Hızlı Aksiyonlar:**\n" +
                        "- Yeni Sınav Ekle\n" +
                        "- Takvime Git\n" +
                        "- Raporları Görüntüle";

                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_dashboard",
                        Label = "Dashboard'a Git",
                        Icon = "home",
                        Path = "/dashboard",
                        Description = "Ana sayfayı görüntüle"
                    });

                    response.PageGuideReference = "dashboard";
                    break;

                case "help":
                    response.Answer = "❓ **Yardım ve Rehberler:**\n\n" +
                        "**1. YasirSharp AI Botu:**\n" +
                        "Sol altta bana her zaman soru sorabilirsiniz! 😊\n\n" +
                        "**2. Sayfa Rehberleri:**\n" +
                        "Her sayfada gerekli açıklamalar ve yönlendirmeler bulunur.\n\n" +
                        "**3. Hızlı Başlangıç:**\n" +
                        "- Sınav eklemek için: Takvime gidin\n" +
                        "- Ders eklemek için: Akademik > Dersler\n" +
                        "- Derslik eklemek için: Akademik > Derslikler\n" +
                        "- Personel eklemek için: Akademik > Personel\n\n" +
                        "**4. Duyurular:**\n" +
                        "Önemli sistem güncellemelerini takip edin.\n\n" +
                        "💡 **İpucu:** Bana istediğiniz soruyu sorabilirsiniz!";

                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_dashboard",
                        Label = "Ana Sayfaya Git",
                        Icon = "home",
                        Path = "/dashboard",
                        Description = "Dashboard'u görüntüle"
                    });

                    response.SuggestedActions.Add(new QuickActionDto
                    {
                        Id = "goto_announcements",
                        Label = "Duyurular",
                        Icon = "bell",
                        Path = "/announcements",
                        Description = "Duyuruları gör"
                    });
                    break;

                default: // general
                    response.Answer = GenerateContextualAnswer(question, pageContext, userRole);
                    response.SuggestedActions = GetContextualActions(pageContext, userRole);
                    break;
            }

            return response;
        }

        /// <summary>
        /// Bağlamsal genel cevap üret
        /// </summary>
        private static string GenerateContextualAnswer(string question, string pageContext, string userRole)
        {
            var contextualGreeting = pageContext switch
            {
                "dashboard" => "Dashboard'da geziniyorsunuz. Size nasıl yardımcı olabilirim?",
                "examSchedule" => "Sınav Takvimi sayfasındasınız. Takvim özellikleri hakkında soru sorabilirsiniz.",
                "courses" => "Dersler sayfasındasınız. Ders yönetimi hakkında yardımcı olabilirim.",
                "exams" => "Sınavlar sayfasındasınız. Sınav işlemleri için size yardımcı olabilirim.",
                "classrooms" => "Derslikler sayfasındasınız. Derslik yönetimi hakkında sorularınızı cevaplayabilirim.",
                "teachers" => "Öğretim Görevlileri sayfasındasınız. Görevli yönetimi için yardımcı olabilirim.",
                "settings" => "Ayarlar sayfasındasınız. Profil ve tercihlerinizi düzenleyebilirsiniz.",
                _ => "Size nasıl yardımcı olabilirim?"
            };

            return $"🤖 Anladım! **\"{question}\"** konusunda size yardımcı olmaya çalışayım.\n\n" +
                   $"{contextualGreeting}\n\n" +
                   "💡 **Size önerebileceklerim:**\n" +
                   "- Daha spesifik bir soru sorabilirsiniz (örn: 'nasıl sınav eklerim?')\n" +
                   "- Aşağıdaki hızlı aksiyonlardan birini kullanabilirsiniz\n" +
                   "- Sayfa rehberini görüntülemek için '?' butonuna tıklayabilirsiniz\n\n" +
                   "Başka nasıl yardımcı olabilirim? 😊";
        }

        /// <summary>
        /// Sayfaya özgü hızlı aksiyonlar öner
        /// </summary>
        private static List<QuickActionDto> GetContextualActions(string pageContext, string userRole)
        {
            var actions = new List<QuickActionDto>();

            // Sayfa bazlı aksiyonlar
            switch (pageContext)
            {
                case "dashboard":
                    actions.Add(new QuickActionDto { Id = "add_exam", Label = "Yeni Sınav Ekle", Icon = "plus-circle", Path = "/calendar", Description = "Hızlıca sınav oluştur" });
                    actions.Add(new QuickActionDto { Id = "view_calendar", Label = "Takvimi Görüntüle", Icon = "calendar", Path = "/calendar", Description = "Sınav takvimini aç" });
                    break;

                case "examSchedule":
                case "calendar":
                    actions.Add(new QuickActionDto { Id = "export_pdf", Label = "PDF İndir", Icon = "download", Path = "/calendar", Description = "Takvimi PDF olarak indir" });
                    actions.Add(new QuickActionDto { Id = "filter", Label = "Filtre Uygula", Icon = "filter", Path = "/calendar", Description = "Derslere göre filtrele" });
                    break;

                case "courses":
                case "dersler":
                    actions.Add(new QuickActionDto { Id = "add_course", Label = "Yeni Ders Ekle", Icon = "book-plus", Path = "/academic/dersler", Description = "Sisteme yeni ders ekle" });
                    break;

                case "exams":
                    actions.Add(new QuickActionDto { Id = "add_exam", Label = "Yeni Sınav", Icon = "plus", Path = "/calendar", Description = "Yeni sınav oluştur" });
                    actions.Add(new QuickActionDto { Id = "export_excel", Label = "Excel İndir", Icon = "file-spreadsheet", Path = "/calendar", Description = "Listeyi Excel'e aktar" });
                    break;

                case "classrooms":
                case "derslikler":
                    actions.Add(new QuickActionDto { Id = "add_classroom", Label = "Yeni Derslik", Icon = "building", Path = "/academic/derslikler", Description = "Derslik ekle" });
                    break;

                case "teachers":
                case "personel":
                    actions.Add(new QuickActionDto { Id = "add_teacher", Label = "Yeni Görevli", Icon = "user-plus", Path = "/academic/personel", Description = "Öğretim görevlisi ekle" });
                    break;

                case "settings":
                    actions.Add(new QuickActionDto { Id = "goto_dashboard", Label = "Ana Sayfa", Icon = "home", Path = "/dashboard", Description = "Dashboard'a dön" });
                    actions.Add(new QuickActionDto { Id = "goto_announcements", Label = "Duyurular", Icon = "bell", Path = "/announcements", Description = "Duyuruları gör" });
                    break;

                default:
                    actions.Add(new QuickActionDto { Id = "dashboard", Label = "Dashboard", Icon = "home", Path = "/dashboard", Description = "Ana sayfaya dön" });
                    actions.Add(new QuickActionDto { Id = "calendar", Label = "Takvim", Icon = "calendar", Path = "/calendar", Description = "Sınav takvimine git" });
                    break;
            }

            return actions;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// String içinde anahtar kelimelerden herhangi birini içeriyor mu?
        /// </summary>
        private static bool ContainsAny(string text, string[] keywords)
        {
            return keywords.Any(keyword => text.Contains(keyword, System.StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }
}
