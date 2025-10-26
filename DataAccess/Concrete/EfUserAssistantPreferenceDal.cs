using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Entity.Concrete;
using System.Linq;

namespace DataAccess.Concrete
{
    /// <summary>
    /// YasirSharp AI - UserAssistantPreference EF Implementation
    /// </summary>
    public class EfUserAssistantPreferenceDal : EfEntityRepositoryBase<UserAssistantPreference, DuzceUniversiteContext>, IUserAssistantPreferenceDal
    {
        public UserAssistantPreference GetByUserId(int userId)
        {
            using (var context = new DuzceUniversiteContext())
            {
                return context.UserAssistantPreferences
                    .FirstOrDefault(x => x.UserId == userId);
            }
        }
    }
}
