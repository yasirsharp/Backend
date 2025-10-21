using Core.DataAccess;
using Entity.Concrete;

namespace DataAccess.Abstract
{
    /// <summary>
    /// AnnouncementReadStatus için Data Access Layer interface
    /// Junction table olduğu için IEntityRepository kullanmıyoruz
    /// </summary>
    public interface IAnnouncementReadStatusDal
    {
        void Add(AnnouncementReadStatus readStatus);
        AnnouncementReadStatus Get(int announcementId, int userId);
        bool Exists(int announcementId, int userId);
    }
}
