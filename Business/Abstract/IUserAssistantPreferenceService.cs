using Core.Utilities.Results;
using Entity.Concrete;

namespace Business.Abstract
{
    /// <summary>
    /// YasirSharp AI - User Preference Service Interface
    /// </summary>
    public interface IUserAssistantPreferenceService
    {
        /// <summary>
        /// Kullanıcının tercihlerini getir
        /// </summary>
        IDataResult<UserAssistantPreference> GetPreference(int userId);
        
        /// <summary>
        /// Tercihleri güncelle
        /// </summary>
        IDataResult<UserAssistantPreference> UpdatePreference(UserAssistantPreference preference);
        
        /// <summary>
        /// Onboarding'i tamamla
        /// </summary>
        IResult CompleteOnboarding(int userId);
        
        /// <summary>
        /// Bot'u aç/kapat
        /// </summary>
        IResult ToggleBot(int userId, bool isEnabled);
        
        /// <summary>
        /// İlk tercihi oluştur (kullanıcı ilk kez sisteme girdiğinde)
        /// </summary>
        IResult CreateDefaultPreference(int userId);
    }
}
