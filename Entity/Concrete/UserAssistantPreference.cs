using Core.Entities;
using System;

namespace Entity.Concrete
{
    /// <summary>
    /// YasirSharp AI - Kullanıcı tercihleri
    /// Her kullanıcının bot ayarları ve onboarding durumu
    /// </summary>
    public class UserAssistantPreference : IEntity
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Kullanıcı ID (User tablosuna referans)
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Bot aktif mi? (Profil ayarlarından açılıp/kapatılabilir)
        /// </summary>
        public bool IsEnabled { get; set; }
        
        /// <summary>
        /// İlk giriş turu tamamlandı mı?
        /// false: Onboarding wizard gösterilir
        /// true: Normal chat açılır
        /// </summary>
        public bool HasCompletedOnboarding { get; set; }
        
        /// <summary>
        /// Son etkileşim tarihi
        /// Analytics için kullanılır
        /// </summary>
        public DateTime? LastInteractionDate { get; set; }
        
        /// <summary>
        /// Tercih edilen dil (tr, en)
        /// Default: "tr"
        /// </summary>
        public string PreferredLanguage { get; set; } = "tr";
        
        // IEntity properties
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Status { get; set; }
    }
}
