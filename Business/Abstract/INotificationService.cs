using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;
using System.Collections.Generic;

namespace Business.Abstract
{
    /// <summary>
    /// Notification (Bildirim) servis interface'i
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Tüm bildirimleri getirir
        /// </summary>
        IDataResult<List<Notification>> GetList();

        /// <summary>
        /// ID'ye göre bildirim getirir
        /// </summary>
        IDataResult<Notification> GetById(int id);

        /// <summary>
        /// Kullanıcıya ait tüm bildirimleri getirir
        /// </summary>
        IDataResult<List<Notification>> GetByUserId(int userId);

        /// <summary>
        /// Kullanıcıya ait okunmamış bildirimleri getirir
        /// </summary>
        IDataResult<List<Notification>> GetUnreadByUserId(int userId);

        /// <summary>
        /// Yeni bildirim ekler
        /// </summary>
        IResult Add(Notification notification);

        /// <summary>
        /// Bildirimi siler
        /// </summary>
        IResult Delete(Notification notification);

        /// <summary>
        /// Bildirimi okundu olarak işaretler
        /// </summary>
        IResult MarkAsRead(int notificationId);

        /// <summary>
        /// Kullanıcının tüm bildirimlerini okundu olarak işaretler
        /// </summary>
        IResult MarkAllAsRead(int userId);

        /// <summary>
        /// Sayfalanmış bildirim listesi döner
        /// </summary>
        IDataResult<PagedResult<Notification>> GetPagedList(PaginationParams paginationParams);
    }
}
