using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using DataAccess.Abstract;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Concrete
{
    public class AnnouncementManager : IAnnouncementService
    {
        private readonly IAnnouncementDal _announcementDal;
        private readonly IAnnouncementReadStatusDal _announcementReadStatusDal;

        public AnnouncementManager(
            IAnnouncementDal announcementDal,
            IAnnouncementReadStatusDal announcementReadStatusDal)
        {
            _announcementDal = announcementDal;
            _announcementReadStatusDal = announcementReadStatusDal;
        }

        public IDataResult<List<Announcement>> GetList()
        {
            var announcements = _announcementDal.GetAllWithBolum();
            return new SuccessDataResult<List<Announcement>>(
                announcements,
                $"{announcements.Count} duyuru bulundu."
            );
        }

        public IDataResult<Announcement> GetById(int announcementId)
        {
            var announcement = _announcementDal.GetByIdWithBolum(announcementId);
            if (announcement == null)
            {
                return new ErrorDataResult<Announcement>(Messages.AnnouncementNotFound);
            }
            return new SuccessDataResult<Announcement>(announcement);
        }

        public IDataResult<List<Announcement>> GetActiveAnnouncements()
        {
            var now = DateTime.Now;
            var activeAnnouncements = _announcementDal.GetAll(a =>
                a.IsActive &&
                a.PublishDate <= now &&
                (a.ExpiryDate == null || a.ExpiryDate > now)
            ).OrderByDescending(a => a.Priority)
              .ThenByDescending(a => a.PublishDate)
              .ToList();

            return new SuccessDataResult<List<Announcement>>(
                activeAnnouncements,
                $"{activeAnnouncements.Count} aktif duyuru bulundu."
            );
        }

        public IDataResult<List<Announcement>> GetByTargetAudience(string targetAudience)
        {
            var now = DateTime.Now;
            var announcements = _announcementDal.GetAll(a =>
                a.IsActive &&
                a.PublishDate <= now &&
                (a.ExpiryDate == null || a.ExpiryDate > now) &&
                (a.TargetAudience == targetAudience || a.TargetAudience == "all")
            ).OrderByDescending(a => a.Priority)
              .ThenByDescending(a => a.PublishDate)
              .ToList();

            return new SuccessDataResult<List<Announcement>>(
                announcements,
                $"{announcements.Count} duyuru bulundu."
            );
        }
        
        /// <summary>
        /// 🆕 Kullanıcı için geçerli duyuruları getir (rol ve bölüm kontrolü ile)
        /// </summary>
        public IDataResult<List<Announcement>> GetByUserId(string targetAudience, int? userBolumId)
        {
            var now = DateTime.Now;
            var announcements = _announcementDal.GetAll(a =>
                a.IsActive &&
                a.PublishDate <= now &&
                (a.ExpiryDate == null || a.ExpiryDate > now) &&
                (a.TargetAudience == targetAudience || a.TargetAudience == "all") &&
                // Bölüm kontrolü: NULL ise herkese, değilse sadece o bölüme
                (a.TargetBolumId == null || a.TargetBolumId == userBolumId)
            ).OrderByDescending(a => a.Priority)
              .ThenByDescending(a => a.PublishDate)
              .ToList();

            return new SuccessDataResult<List<Announcement>>(
                announcements,
                $"{announcements.Count} duyuru bulundu."
            );
        }

        public IDataResult<List<Announcement>> GetPopupAnnouncements(string targetAudience)
        {
            var now = DateTime.Now;
            
            // Aktif popup duyurularını getir
            var popupAnnouncements = _announcementDal.GetAll(a =>
                a.IsActive &&
                a.ShowAsPopup &&
                a.PublishDate <= now &&
                (a.ExpiryDate == null || a.ExpiryDate > now) &&
                (a.TargetAudience == targetAudience || a.TargetAudience == "all")
            ).OrderByDescending(a => a.Priority)
              .ThenByDescending(a => a.PublishDate)
              .ToList();

            return new SuccessDataResult<List<Announcement>>(
                popupAnnouncements,
                $"{popupAnnouncements.Count} popup duyuru bulundu."
            );
        }
        
        /// <summary>
        /// 🆕 Popup duyuruları getir (bölüm kontrolü ile)
        /// </summary>
        public IDataResult<List<Announcement>> GetPopupAnnouncementsByUser(string targetAudience, int? userBolumId)
        {
            var now = DateTime.Now;
            
            var popupAnnouncements = _announcementDal.GetAll(a =>
                a.IsActive &&
                a.ShowAsPopup &&
                a.PublishDate <= now &&
                (a.ExpiryDate == null || a.ExpiryDate > now) &&
                (a.TargetAudience == targetAudience || a.TargetAudience == "all") &&
                // Bölüm kontrolü
                (a.TargetBolumId == null || a.TargetBolumId == userBolumId)
            ).OrderByDescending(a => a.Priority)
              .ThenByDescending(a => a.PublishDate)
              .ToList();

            return new SuccessDataResult<List<Announcement>>(
                popupAnnouncements,
                $"{popupAnnouncements.Count} popup duyuru bulundu."
            );
        }

        public IResult Add(Announcement announcement)
        {
            _announcementDal.Add(announcement);
            return new SuccessResult(Messages.AnnouncementAdded);
        }
        
        /// <summary>
        /// 🆕 Duyuru ekle (yetki kontrolü ile)
        /// Görevli Personel sadece kendi bölümüne duyuru gönderebilir
        /// </summary>
        public IResult AddWithPermission(Announcement announcement, string userRole, int? userBolumId)
        {
            // Görevli Personel kontrolü
            if (userRole == "gorevli.personel")
            {
                // Görevli Personel sadece kendi bölümüne duyuru gönderebilir
                if (announcement.TargetBolumId == null)
                {
                    return new ErrorResult("Görevli Personel tüm bölümlere duyuru gönderemez. Lütfen kendi bölümünüzü seçin.");
                }
                
                if (announcement.TargetBolumId != userBolumId)
                {
                    return new ErrorResult("Sadece kendi bölümünüze duyuru gönderebilirsiniz.");
                }
            }
            
            // Admin/Super.Admin herhangi bir bölüme gönderebilir
            _announcementDal.Add(announcement);
            return new SuccessResult(Messages.AnnouncementAdded);
        }

        public IResult Update(Announcement announcement)
        {
            var existingAnnouncement = _announcementDal.Get(a => a.Id == announcement.Id);
            if (existingAnnouncement == null)
            {
                return new ErrorResult(Messages.AnnouncementNotFound);
            }

            _announcementDal.Update(announcement);
            return new SuccessResult(Messages.AnnouncementUpdated);
        }

        public IResult Delete(Announcement announcement)
        {
            _announcementDal.Delete(announcement);
            return new SuccessResult(Messages.AnnouncementDeleted);
        }

        public IResult MarkAsRead(int announcementId, int userId)
        {
            var announcement = _announcementDal.Get(a => a.Id == announcementId);
            if (announcement == null)
            {
                return new ErrorResult(Messages.AnnouncementNotFound);
            }

            // Daha önce okunmuş mu kontrol et
            if (_announcementReadStatusDal.Exists(userId, announcementId))
            {
                return new SuccessResult("Duyuru zaten okunmuş olarak işaretli.");
            }

            // Okuma kaydını ekle
            var readStatus = new AnnouncementReadStatus
            {
                AnnouncementId = announcementId,
                UserId = userId,
                ReadDate = DateTime.Now
            };
            _announcementReadStatusDal.Add(readStatus);

            return new SuccessResult(Messages.AnnouncementMarkedAsRead);
        }

        public IDataResult<bool> HasUserRead(int announcementId, int userId)
        {
            var hasRead = _announcementReadStatusDal.Exists(userId, announcementId);
            return new SuccessDataResult<bool>(hasRead);
        }

        public IDataResult<PagedResult<Announcement>> GetPagedList(PaginationParams paginationParams)
        {
            PagedResult<Announcement> pagedResult;

            // Arama terimi varsa filtrele
            if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
            {
                pagedResult = _announcementDal.GetPaged(
                    paginationParams,
                    a => a.Title.Contains(paginationParams.SearchTerm) ||
                         a.Content.Contains(paginationParams.SearchTerm)
                );
            }
            else
            {
                pagedResult = _announcementDal.GetPaged(paginationParams);
            }

            return new SuccessDataResult<PagedResult<Announcement>>(
                pagedResult,
                $"{pagedResult.TotalCount} duyuru bulundu."
            );
        }
    }
}
