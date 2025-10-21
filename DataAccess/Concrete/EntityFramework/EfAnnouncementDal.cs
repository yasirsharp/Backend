using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entity.Concrete;

namespace DataAccess.Concrete.EntityFramework
{
    /// <summary>
    /// Announcement entity'si için Entity Framework Data Access implementasyonu
    /// </summary>
    public class EfAnnouncementDal : EfEntityRepositoryBase<Announcement, DuzceUniversiteContext>, IAnnouncementDal
    {
    }
}
