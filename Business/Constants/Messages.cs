using Core.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Constants
{
    public static class Messages
    {
        public static string AkademikPersonelAdded = "Akademik Personel Eklendi.";
        public static string AkademikPersonelUpdated = "Akademik Personel Güncellendi.";
        public static string AkademikPersonelDeleted = "Akademik Personel Silindi.";
        public static string AkademikPersonelNotFound = "Akademik Personel bulunamadı.";
        public static string AkademikPersonelNotFoundForUser = "Bu kullanıcıya ait akademik personel kaydı bulunamadı.";
        public static string AkademikPersonelFound = "Akademik Personel bulundu.";

        public static string BolumAdded = "Bölüm Eklendi.";
        public static string BolumDeleted = "Bölüm Silindi.";
        public static string BolumUpdated = "Bölüm Güncellendi";

        public static string DersAdded = "Ders Eklendi.";
        public static string DersDeleted = "Ders Silindi.";
        public static string DersUpdated = "Ders Güncellendi.";

        public static string DersBolumAdded = "Ders-Bölüm ilişkisi eklendi.";
        public static string DersBolumDeleted = "Ders-Bölüm ilişkisi silindi.";
        public static string DersBolumUpdated = "Ders-Bölüm ilişkisi güncellendi.";

        public static string DBAPAdded = "Ders Bölüm Akademik Personel Eşleştirmesi yapıldı";
        public static string DBAPDeleted = "Ders Bölüm Akademik Personel Eşleştirmesi silindi!!";
        public static string DBAPUpdated = "Ders Bölüm Akademik Personel Eşleştirmesi güncellendi";

        public static string DerslikAdded = "Derslik Eklendi.";
        public static string DerslikDeleted = "Derslik Silindi.";
        public static string DerslikUpdated = "Derslik Güncellendi.";

        public static string OgrenciAdded = "Öğrenci Eklendi.";
        public static string OgrenciDeleted = "Öğrenci Silindi.";
        public static string OgrenciUpdated = "Öğrenci Güncellendi.";
        public static string OgrenciNotFound = "Öğrenci bulunamadı.";


        public static string SomethingWrong = "Bir şeyler yanlış gidiyor. @?!!#";

        public static string SinavDetayAdded ="Sinav Detay Eşleştirmesi eklendi.";
        public static string SinavDetayDeleted ="Sinav Detay Eşleştirmesi silindi.";
        public static string SinavDetayUpdated ="Sinav Detay Eşleştirmesi güncellendi.";
        public static string SinavDetayNotFound ="Sınav kaydı bulunamadı.";

        public static string SinavDerslikAdded ="Sinav Derslik Eşleştirmesi eklendi.";
        public static string SinavDerslikDeleted = "Sinav Derslik Eşleştirmesi silindi.";
        public static string SinavDerslikUpdated ="Sinav Derslik Eşleştirmesi güncellendi.";

        public static string UserAdded = "Kullanıcı başarıyla eklendi.";
        public static string UserDeleted = "Kullanıcı silindi.";
        public static string UserNotFound = "Kullanıcı bulunamadı.";
        public static string UserClaimsNotFound = "Kullanıcıya ait yetkiler bulunamadı.";

        public static string AccessTokenCreated = "Access token başarıyla oluşturuldu.";
        public static string UserAlreadyExists = "Bu e-posta adresi ile kayıtlı bir kullanıcı zaten mevcut.";
        public static string SuccessfulLogin = "Giriş başarılı.";
        public static string UserRegistered = "Kullanıcı başarıyla kaydedildi.";
        public static string PasswordError = "Şifre hatalı.";
        
        public static string OperationClaimAdded = "Yetki başarıyla eklendi.";
        public static string OperationClaimUpdated = "Yetki başarıyla güncellendi.";
        public static string OperationClaimListed = "Yetkiler listelendi.";
        public static string OperationClaimDeleted = "Yetki başarıyla silindi.";
        
        public static string UserOperationClaimAdded = "Kullanıcı yetkisi başarıyla eklendi.";
        public static string UserOperationClaimDeleted = "Kullanıcı yetkisi başarıyla silindi.";
        public static string UserOperationClaimListed = "Kullanıcı yetkileri listelendi.";
        public static string UserOperationClaimUpdated = "Kullanıcı yetkisi başarıyla güncellendi.";

        public static string AuthorizationDenied = "Yetkiniz Yok!!!";

        public static string UserUpdated = "Kullanıcı güncellendi.";

        // Notification Messages
        public static string NotificationAdded = "Bildirim eklendi.";
        public static string NotificationDeleted = "Bildirim silindi.";
        public static string NotificationNotFound = "Bildirim bulunamadı.";
        public static string NotificationMarkedAsRead = "Bildirim okundu olarak işaretlendi.";

        // Announcement Messages
        public static string AnnouncementAdded = "Duyuru eklendi.";
        public static string AnnouncementUpdated = "Duyuru güncellendi.";
        public static string AnnouncementDeleted = "Duyuru silindi.";
        public static string AnnouncementNotFound = "Duyuru bulunamadı.";
        public static string AnnouncementMarkedAsRead = "Duyuru okundu olarak işaretlendi.";

        // YasirSharp AI - Assistant Messages (26 Ekim 2025)
        public static string InteractionLogged = "Etkileşim kaydedildi.";
        public static string PageGuideNotFound = "Sayfa rehberi bulunamadı.";
        public static string PreferenceNotFound = "Kullanıcı tercihi bulunamadı.";
        public static string PreferenceCreated = "Kullanıcı tercihi oluşturuldu.";
        public static string PreferenceUpdated = "Kullanıcı tercihi güncellendi.";
        public static string OnboardingCompleted = "Giriş turu tamamlandı.";
        public static string BotEnabled = "Yardımcı bot etkinleştirildi.";
        public static string BotDisabled = "Yardımcı bot devre dışı bırakıldı.";
    }
}
