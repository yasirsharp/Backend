using Core.DataAccess;
using Entity.Concrete;

namespace DataAccess.Abstract
{
    /// <summary>
    /// Announcement entity'si için Data Access Layer interface
    /// </summary>
    public interface IAnnouncementDal : IEntityRepository<Announcement>
    {
        // Özel metodlar eklenebilir (örn: GetActiveAnnouncements)
    }
}
