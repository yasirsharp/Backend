using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using DataAccess.Abstract;
using Entity.Concrete;
using System.Linq.Expressions;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Core.CrossCuttingConcerns.Validation;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Validation;
using Core.Utilities.Business;
using Business.BusinessAspects.Autofac;
using Core.Aspects.Autofac.Caching;

namespace Business.Concrete
{
    public class AkademikPersonelManager : IAkademikPersonelService
    {
        private readonly IAkademikPersonelDal _akademikPersonelDal;
        private readonly IUserService _userService;
        IDBAPDal _dBAPDal;
        ISinavDetayDal _sinavDetayDal;

        public AkademikPersonelManager(
            IAkademikPersonelDal akademikPersonelDal, 
            IDBAPDal dBAPDal,
            ISinavDetayDal sinavDetayDal,
            IUserService userService)
        {
            _akademikPersonelDal = akademikPersonelDal;
            _dBAPDal = dBAPDal;
            _sinavDetayDal = sinavDetayDal;
            _userService = userService;
        }

        //[SecuredOperation("akademik.personel")]
        [ValidationAspect(typeof(AkademikPersonelValidator))]
        public IResult Add(AkademikPersonel akademikPersonel)
        {
            // ESKI: Deprecated metod kullanıyordu (iş kuralları Dal'daydı)
            // ŞİMDİ: Business layer'da iş kuralları, Dal'da sadece transaction
            
            // AdminPanel için: Credentials otomatik oluşturulacak
            // Ama önce AkademikPersonel kaydedilmeli (ID için)
            _akademikPersonelDal.Add(akademikPersonel);
            
            try
            {
                // İŞ KURALI: Username ve Password otomatik oluştur (AdminPanel pattern)
                var nameParts = akademikPersonel.Ad.Split(' ');
                var lastName = nameParts.Length > 1 ? nameParts.Last() : akademikPersonel.Ad;
                var firstName = akademikPersonel.Ad.Replace(" " + nameParts.Last(), "");
                
                var initials = string.Join("", nameParts.Select(p => p[0]));
                var userName = $"{akademikPersonel.Id}{initials}";
                
                byte[] passwordHash, passwordSalt;
                Core.Utilities.Security.Hashing.HashingHelper.CreatePasswordHash(userName, out passwordHash, out passwordSalt);
                
                var user = new Core.Entities.Concrete.User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    UserName = userName,
                    Email = userName + "@duzce.edu.tr",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Status = true
                };
                
                var userResult = _userService.Add(user);
                if (!userResult.Success)
                {
                    // Rollback: AkademikPersonel'i sil
                    _akademikPersonelDal.Delete(akademikPersonel);
                    return new ErrorResult(userResult.Message);
                }
                
                // AkademikPersonel'e UserId ata ve güncelle
                akademikPersonel.UserId = user.Id;
                _akademikPersonelDal.Update(akademikPersonel);
                
                // Role ata (OperationClaimId = 3)
                var userOperationClaim = new Core.Entities.Concrete.UserOperationClaim
                {
                    UserId = user.Id,
                    OperationClaimId = 2, // "akademik.personel" role'ü
                    CreatedDate = DateTime.Now,
                    Status = true
                };
                
                // UserOperationClaim için service gerek (şimdilik direkt Dal kullan)
                // TODO: IUserOperationClaimService dependency ekle
                var context = new DataAccess.Concrete.EntityFramework.DuzceUniversiteContext();
                context.UserOperationClaims.Add(userOperationClaim);
                context.SaveChanges();
                
                return new SuccessResult(Messages.AkademikPersonelAdded);
            }
            catch (Exception ex)
            {
                // Rollback: AkademikPersonel'i sil
                _akademikPersonelDal.Delete(akademikPersonel);
                return new ErrorResult("Akademik personel eklenirken hata oluştu: " + ex.Message);
            }
        }

        private IResult CheckIfPersonelExists(int akademikPersonelId)
        {
            var result = _akademikPersonelDal.Get(q => q.Id == akademikPersonelId);
            if (result == null)
            {
                return new ErrorResult(Messages.AkademikPersonelNotFound);
            }
            return new SuccessResult();
        }


        public IResult Delete(AkademikPersonel akademikPersonel)
        {
            IResult result = BusinessRules.Run(CheckIfPersonelExists(akademikPersonel.Id),
                EslesmeSorgusu(akademikPersonel));

            if (result != null)
            {
                return result;
            }
            
            // Yeni async metodu kullan - Dal sadece transaction yapar
            try
            {
                _akademikPersonelDal.DeleteAkademikPersonelWithUserOperationClaimAsync(akademikPersonel).Wait();
                return new SuccessResult(Messages.AkademikPersonelDeleted);
            }
            catch (Exception ex)
            {
                return new ErrorResult("Akademik personel silinirken hata oluştu: " + ex.Message);
            }
        }

        private IResult EslesmeSorgusu(AkademikPersonel akademikPersonel)
        {
            var dbap = _dBAPDal.GetDetails(q => q.AkademikPersonelId == akademikPersonel.Id);
            if (dbap.Count > 0)
            {
                string message = $"{akademikPersonel.Ad} için {dbap.Count} tane Bölüm-Ders-Akademik Personel Eşleştirmesi bulunmaktadır.\n";
                foreach (var item in dbap)
                {
                    message += $"{item.BolumAd} \n{item.DersAd} \n{item.AkademikPersonelAd} ({item.Unvan})\n";
                }
                return new ErrorResult(message);
            }
            return new SuccessResult();
        }

        public IDataResult<AkademikPersonel> GetById(int akademikPeronelId)
        {
            return new SuccessDataResult<AkademikPersonel>(_akademikPersonelDal.Get(q => q.Id == akademikPeronelId));
        }

        public IDataResult<AkademikPersonel> GetByUserId(int userId)
        {
            var akademikPersonel = _akademikPersonelDal.Get(q => q.UserId == userId);
            if (akademikPersonel == null)
            {
                return new ErrorDataResult<AkademikPersonel>(Messages.AkademikPersonelNotFoundForUser);
            }
            return new SuccessDataResult<AkademikPersonel>(akademikPersonel, Messages.AkademikPersonelFound);
        }

        public IDataResult<List<AkademikPersonel>> GetList(Expression<Func<AkademikPersonel, bool>> filter = null)
        {
            return new SuccessDataResult<List<AkademikPersonel>>(_akademikPersonelDal.GetAll(filter), $"{_akademikPersonelDal.GetAll(filter).Count} tane bulundu.");
        }

        public IDataResult<PagedResult<AkademikPersonel>> GetPagedList(PaginationParams paginationParams)
        {
            var searchTerm = paginationParams.SearchTerm?.ToLower();
            Expression<Func<AkademikPersonel, bool>> filter = null;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filter = ap => ap.Ad.ToLower().Contains(searchTerm);
            }

            var pagedResult = _akademikPersonelDal.GetPaged(paginationParams, filter);
            return new SuccessDataResult<PagedResult<AkademikPersonel>>(pagedResult, $"Toplam {pagedResult.TotalCount} akademik personel bulundu.");
        }

        public IResult Update(AkademikPersonel akademikPersonel)
        {
            // İŞ KURALI: User bilgilerini güncelle (AdminPanel pattern)
            try
            {
                var nameParts = akademikPersonel.Ad.Split(' ');
                var lastName = nameParts.Length > 1 ? nameParts.Last() : akademikPersonel.Ad;
                var firstName = akademikPersonel.Ad.Replace(" " + nameParts.Last(), "");
                
                var initials = string.Join("", nameParts.Select(p => p[0]));
                var userName = $"{akademikPersonel.Id}{initials}";
                
                byte[] passwordHash, passwordSalt;
                Core.Utilities.Security.Hashing.HashingHelper.CreatePasswordHash(userName, out passwordHash, out passwordSalt);
                
                // Mevcut User'ı bul
                var userResult = _userService.GetById(akademikPersonel.UserId);
                if (!userResult.Success)
                {
                    return new ErrorResult("Kullanıcı bulunamadı: " + userResult.Message);
                }
                
                var user = userResult.Data;
                user.FirstName = firstName;
                user.LastName = lastName;
                user.UserName = userName;
                user.Email = userName + "@duzce.edu.tr";
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.Status = true;
                user.UpdatedDate = DateTime.Now;

                akademikPersonel.UpdatedDate = DateTime.Now;

                // Yeni async metodu kullan - Business'tan HAZIR nesneler gönder
                _akademikPersonelDal.UpdateAkademikPersonelWithUserOperationClaimAsync(user, akademikPersonel).Wait();
                
                return new SuccessResult(Messages.AkademikPersonelUpdated);
            }
            catch (Exception ex)
            {
                return new ErrorResult("Akademik personel güncellenirken hata oluştu: " + ex.Message);
            }
        }
        
    }
}
