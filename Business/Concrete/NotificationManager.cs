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
    public class NotificationManager : INotificationService
    {
        private readonly INotificationDal _notificationDal;

        public NotificationManager(INotificationDal notificationDal)
        {
            _notificationDal = notificationDal;
        }

        public IDataResult<List<Notification>> GetList()
        {
            var notifications = _notificationDal.GetAll();
            return new SuccessDataResult<List<Notification>>(
                notifications,
                $"{notifications.Count} bildirim bulundu."
            );
        }

        public IDataResult<Notification> GetById(int notificationId)
        {
            var notification = _notificationDal.Get(n => n.Id == notificationId);
            if (notification == null)
            {
                return new ErrorDataResult<Notification>(Messages.NotificationNotFound);
            }
            return new SuccessDataResult<Notification>(notification);
        }

        public IDataResult<List<Notification>> GetByUserId(int userId)
        {
            var notifications = _notificationDal.GetAll(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .ToList();
            return new SuccessDataResult<List<Notification>>(
                notifications,
                $"{notifications.Count} bildirim bulundu."
            );
        }

        public IDataResult<List<Notification>> GetUnreadByUserId(int userId)
        {
            var unreadNotifications = _notificationDal.GetAll(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedDate)
                .ToList();
            return new SuccessDataResult<List<Notification>>(
                unreadNotifications,
                $"{unreadNotifications.Count} okunmamış bildirim bulundu."
            );
        }

        public IResult Add(Notification notification)
        {
            _notificationDal.Add(notification);
            return new SuccessResult(Messages.NotificationAdded);
        }

        public IResult Delete(Notification notification)
        {
            _notificationDal.Delete(notification);
            return new SuccessResult(Messages.NotificationDeleted);
        }

        public IResult MarkAsRead(int notificationId)
        {
            var notification = _notificationDal.Get(n => n.Id == notificationId);
            if (notification == null)
            {
                return new ErrorResult(Messages.NotificationNotFound);
            }

            notification.IsRead = true;
            notification.ReadDate = DateTime.Now;
            _notificationDal.Update(notification);

            return new SuccessResult(Messages.NotificationMarkedAsRead);
        }

        public IResult MarkAllAsRead(int userId)
        {
            var unreadNotifications = _notificationDal.GetAll(n => n.UserId == userId && !n.IsRead);
            
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadDate = DateTime.Now;
                _notificationDal.Update(notification);
            }

            return new SuccessResult($"{unreadNotifications.Count} bildirim okundu olarak işaretlendi.");
        }

        public IDataResult<PagedResult<Notification>> GetPagedList(PaginationParams paginationParams)
        {
            PagedResult<Notification> pagedResult;

            // Arama terimi varsa filtrele
            if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
            {
                pagedResult = _notificationDal.GetPaged(
                    paginationParams,
                    n => n.Title.Contains(paginationParams.SearchTerm) ||
                         n.Message.Contains(paginationParams.SearchTerm)
                );
            }
            else
            {
                pagedResult = _notificationDal.GetPaged(paginationParams);
            }

            return new SuccessDataResult<PagedResult<Notification>>(
                pagedResult,
                $"{pagedResult.TotalCount} bildirim bulundu."
            );
        }
    }
}
