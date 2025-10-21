using Core.DataAccess;
using Entity.Concrete;

namespace DataAccess.Abstract
{
    /// <summary>
    /// Notification entity'si için Data Access Layer interface
    /// </summary>
    public interface INotificationDal : IEntityRepository<Notification>
    {
        // Özel metodlar eklenebilir (örn: GetUnreadByUserId)
    }
}
