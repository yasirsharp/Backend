using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entity.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess.Concrete.EntityFramework
{
    /// <summary>
    /// Announcement entity'si için Entity Framework Data Access implementasyonu
    /// </summary>
    public class EfAnnouncementDal : EfEntityRepositoryBase<Announcement, DuzceUniversiteContext>, IAnnouncementDal
    {
        public List<Announcement> GetAllWithBolum()
        {
            using (var context = new DuzceUniversiteContext())
            {
                return context.Announcements
                    .Include(a => a.TargetBolum)
                    .ToList();
            }
        }

        public Announcement GetByIdWithBolum(int id)
        {
            using (var context = new DuzceUniversiteContext())
            {
                return context.Announcements
                    .Include(a => a.TargetBolum)
                    .FirstOrDefault(a => a.Id == id);
            }
        }
    }
}
