using Core.DataAccess;
using Entity.Concrete;
using System.Collections.Generic;

namespace DataAccess.Abstract
{
    /// <summary>
    /// Announcement entity'si için Data Access Layer interface
    /// </summary>
    public interface IAnnouncementDal : IEntityRepository<Announcement>
    {
        /// <summary>
        /// Bölüm bilgisi ile birlikte tüm duyuruları getirir
        /// </summary>
        List<Announcement> GetAllWithBolum();
        
        /// <summary>
        /// Bölüm bilgisi ile birlikte ID'ye göre duyuru getirir
        /// </summary>
        Announcement GetByIdWithBolum(int id);
    }
}
