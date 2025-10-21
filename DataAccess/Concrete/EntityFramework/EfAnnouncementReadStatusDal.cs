using DataAccess.Abstract;
using Entity.Concrete;
using System.Linq;

namespace DataAccess.Concrete.EntityFramework
{
    /// <summary>
    /// AnnouncementReadStatus için Entity Framework Data Access implementasyonu
    /// Junction table olduğu için özel implementasyon
    /// </summary>
    public class EfAnnouncementReadStatusDal : IAnnouncementReadStatusDal
    {
        public void Add(AnnouncementReadStatus readStatus)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                context.AnnouncementReadStatus.Add(readStatus);
                context.SaveChanges();
            }
        }

        public AnnouncementReadStatus Get(int announcementId, int userId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                return context.AnnouncementReadStatus
                    .FirstOrDefault(x => x.AnnouncementId == announcementId && x.UserId == userId);
            }
        }

        public bool Exists(int announcementId, int userId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                return context.AnnouncementReadStatus
                    .Any(x => x.AnnouncementId == announcementId && x.UserId == userId);
            }
        }
    }
}
