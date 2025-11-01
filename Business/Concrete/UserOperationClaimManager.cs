using Business.Abstract;
using Business.Constants;
using Core.Entities.Concrete;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Concrete
{
    public class UserOperationClaimManager : IUserOperationClaimService
    {
        private readonly IUserOperationClaimDal _userOperationClaimDal;
        private readonly IUserService _userService;
        private readonly IOperationClaimDal _operationClaimDal;
        private readonly IAkademikPersonelDal _akademikPersonelDal;
        private readonly IBolumAkademikPersonellerDal _bolumAkademikPersonellerDal;

        public UserOperationClaimManager(
            IUserOperationClaimDal userOperationClaimDal, 
            IUserService userService,
            IOperationClaimDal operationClaimDal,
            IAkademikPersonelDal akademikPersonelDal,
            IBolumAkademikPersonellerDal bolumAkademikPersonellerDal)
        {
            _userOperationClaimDal = userOperationClaimDal;
            _userService = userService;
            _operationClaimDal = operationClaimDal;
            _akademikPersonelDal = akademikPersonelDal;
            _bolumAkademikPersonellerDal = bolumAkademikPersonellerDal;
        }

        public IResult Add(UserOperationClaim userOperationClaim)
        {
            try
            {
                _userOperationClaimDal.Add(userOperationClaim);
                return new SuccessResult(Messages.UserOperationClaimAdded);
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Kullanıcı yetkisi eklenirken bir hata oluştu: {ex.Message}");
            }
        }

        public IResult Delete(UserOperationClaim userOperationClaim)
        {
            try
            {
                // ✅ YENİ: "bolum.gorevlisi" role'ü siliniyorsa kontrol yap
                var role = _operationClaimDal.Get(r => r.Id == userOperationClaim.OperationClaimId);
                
                if (role != null && role.Name == "bolum.gorevlisi")
                {
                    // Bu kullanıcının akademik personel kaydını bul
                    var akademikPersonel = _akademikPersonelDal.Get(ap => ap.UserId == userOperationClaim.UserId);
                    
                    if (akademikPersonel != null)
                    {
                        // Aktif bölüm atamalarını kontrol et
                        var activeAssignments = _bolumAkademikPersonellerDal.GetAll(bap => 
                            bap.AkademikPersonelId == akademikPersonel.Id && 
                            bap.Status == true
                        );
                        
                        if (activeAssignments.Count > 0)
                        {
                            return new ErrorResult(
                                "Bu kullanıcının aktif bölüm atamaları bulunmaktadır. " +
                                "Önce bölüm atamalarını kaldırmanız gerekmektedir."
                            );
                        }
                    }
                }

                _userOperationClaimDal.Delete(userOperationClaim);
                return new SuccessResult(Messages.UserOperationClaimDeleted);
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Kullanıcı yetkisi silinirken bir hata oluştu: {ex.Message}");
            }
        }

        public IDataResult<List<UserOperationClaim>> GetAll()
        {
            try
            {
                var claims = _userOperationClaimDal.GetAll();
                return new SuccessDataResult<List<UserOperationClaim>>(claims, Messages.UserOperationClaimListed);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<UserOperationClaim>>($"Kullanıcı yetkileri listelenirken bir hata oluştu: {ex.Message}");
            }
        }

        public IDataResult<UserOperationClaim> GetById(int id)
        {
            try
            {
                var claim = _userOperationClaimDal.Get(c => c.Id == id);
                if (claim == null)
                {
                    return new ErrorDataResult<UserOperationClaim>("Kullanıcı yetkisi bulunamadı.");
                }
                return new SuccessDataResult<UserOperationClaim>(claim, Messages.UserOperationClaimListed);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<UserOperationClaim>($"Kullanıcı yetkisi getirilirken bir hata oluştu: {ex.Message}");
            }
        }

        public IDataResult<List<UserOperationClaim>> GetByUserId(int userId)
        {
            try
            {
                var claims = _userOperationClaimDal.GetAll(c => c.UserId == userId);
                return new SuccessDataResult<List<UserOperationClaim>>(claims, Messages.UserOperationClaimListed);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<UserOperationClaim>>($"Kullanıcı yetkileri listelenirken bir hata oluştu: {ex.Message}");
            }
        }
        public IResult Update(UserOperationClaim userOperationClaim)
        {
            try
            {
                _userOperationClaimDal.Update(userOperationClaim);
                return new SuccessResult(Messages.UserOperationClaimUpdated);
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Kullanıcı yetkisi güncellenirken bir hata oluştu: {ex.Message}");
            }
        }
    }
} 