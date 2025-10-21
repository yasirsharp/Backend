using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entity.Concrete;

namespace DataAccess.Concrete.EntityFramework
{
    /// <summary>
    /// Notification entity'si için Entity Framework Data Access implementasyonu
    /// </summary>
    public class EfNotificationDal : EfEntityRepositoryBase<Notification, DuzceUniversiteContext>, INotificationDal
    {
    }
}
