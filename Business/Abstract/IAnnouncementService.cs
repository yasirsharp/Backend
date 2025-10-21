using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;
using System.Collections.Generic;

namespace Business.Abstract
{
    /// <summary>
    /// Announcement (Duyuru) servis interface'i
    /// </summary>
    public interface IAnnouncementService
    {
        /// <summary>
        /// Tüm duyuruları getirir
        /// </summary>
        IDataResult<List<Announcement>> GetList();

        /// <summary>
        /// ID'ye göre duyuru getirir
        /// </summary>
        IDataResult<Announcement> GetById(int id);

        /// <summary>
        /// Aktif duyuruları getirir (IsActive=true ve yayın tarihleri uygun)
        /// </summary>
        IDataResult<List<Announcement>> GetActiveAnnouncements();

        /// <summary>
        /// Hedef kitleye göre aktif duyuruları getirir
        /// </summary>
        IDataResult<List<Announcement>> GetByTargetAudience(string role);

        /// <summary>
        /// Popup olarak gösterilecek duyuruları getirir
        /// </summary>
        IDataResult<List<Announcement>> GetPopupAnnouncements(string role);

        /// <summary>
        /// Yeni duyuru ekler
        /// </summary>
        IResult Add(Announcement announcement);

        /// <summary>
        /// Duyuru günceller
        /// </summary>
        IResult Update(Announcement announcement);

        /// <summary>
        /// Duyuru siler
        /// </summary>
        IResult Delete(Announcement announcement);

        /// <summary>
        /// Duyuruyu okundu olarak işaretler
        /// </summary>
        IResult MarkAsRead(int announcementId, int userId);

        /// <summary>
        /// Kullanıcı duyuruyu okudu mu?
        /// </summary>
        IDataResult<bool> HasUserRead(int announcementId, int userId);

        /// <summary>
        /// Sayfalanmış duyuru listesi döner
        /// </summary>
        IDataResult<PagedResult<Announcement>> GetPagedList(PaginationParams paginationParams);
    }
}
