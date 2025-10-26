using Core.DataAccess;
using Entity.Concrete;

namespace DataAccess.Abstract
{
    /// <summary>
    /// YasirSharp AI - UserAssistantPreference Data Access Layer
    /// </summary>
    public interface IUserAssistantPreferenceDal : IEntityRepository<UserAssistantPreference>
    {
        /// <summary>
        /// Kullanıcının tercihlerini getirir
        /// </summary>
        UserAssistantPreference GetByUserId(int userId);
    }
}
