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
        /// 🆕 Kullanıcı için geçerli duyuruları getirir (rol ve bölüm kontrolü ile)
        /// </summary>

        /// <summary>
        /// Popup olarak gösterilecek duyuruları getirir
        /// </summary>
        IDataResult<List<Announcement>> GetPopupAnnouncements(string role);
        
        /// <summary>
        /// 🆕 Popup duyuruları getir (bölüm kontrolü ile)
        /// </summary>

        /// <summary>
        /// Yeni duyuru ekler
        /// </summary>
        /// <summary>
        /// Kullanıcının görebileceği yayındaki duyurular: hedef kitlelerinden biri (veya "all") ve
        /// bölümsüz ya da kullanıcının bölümlerinden birine yönelik. popupOnly: okunmamış açılır duyurular.
        /// </summary>
        IDataResult<List<Announcement>> GetForUser(IReadOnlyCollection<string> audiences, IReadOnlyCollection<int> bolumIds,
            int userId, bool popupOnly);

        /// <summary>Başlık, içerik, tür, hedef kitle ve tarihleri doğrular.</summary>
        IResult Validate(Announcement announcement);

        IResult Add(Announcement announcement);
        
        /// <summary>
        /// 🆕 Yeni duyuru ekle (yetki kontrolü ile)
        /// Görevli Personel sadece kendi bölümüne duyuru gönderebilir
        /// </summary>

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
