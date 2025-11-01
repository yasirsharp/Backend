using Business.Abstract;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entity.Concrete;
using Entity.DTOs;
using Core.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class BolumAkademikPersonelManager : IBolumAkademikPersonellerService
    {
        IBolumAkademikPersonellerDal _bolumAkademikPersonellerDal;
        IUserDal _userDal;
        IUserOperationClaimDal _userOperationClaimDal;
        IOperationClaimDal _operationClaimDal;
        IAkademikPersonelDal _akademikPersonelDal;

        public BolumAkademikPersonelManager(
            IBolumAkademikPersonellerDal bolumAkademikPersonellerDal,
            IUserDal userDal,
            IUserOperationClaimDal userOperationClaimDal,
            IOperationClaimDal operationClaimDal,
            IAkademikPersonelDal akademikPersonelDal)
        {
            _bolumAkademikPersonellerDal = bolumAkademikPersonellerDal;
            _userDal = userDal;
            _userOperationClaimDal = userOperationClaimDal;
            _operationClaimDal = operationClaimDal;
            _akademikPersonelDal = akademikPersonelDal;
        }

        public IResult Add(BolumAkademikPersoneller bolumAkademikPersoneller)
        {
            try
            {
                Console.WriteLine($"[BolumAkademikPersonel] Add işlemi başladı - AkademikPersonelId: {bolumAkademikPersoneller.AkademikPersonelId}, BolumId: {bolumAkademikPersoneller.BolumId}");
                
                // Önce atamayı kaydet
                _bolumAkademikPersonellerDal.Add(bolumAkademikPersoneller);
                Console.WriteLine($"[BolumAkademikPersonel] Atama kaydedildi");

                // ✅ YENİ: Akademik personelin UserId'sini bul
                var akademikPersonel = _akademikPersonelDal.Get(ap => ap.Id == bolumAkademikPersoneller.AkademikPersonelId);
                Console.WriteLine($"[BolumAkademikPersonel] AkademikPersonel bulundu: {akademikPersonel != null}, UserId: {akademikPersonel?.UserId}");
                
                if (akademikPersonel != null && akademikPersonel.UserId > 0)
                {
                    // "bolum.gorevlisi" role'ünü bul
                    var gorevliRole = _operationClaimDal.Get(r => r.Name == "bolum.gorevlisi");
                    Console.WriteLine($"[BolumAkademikPersonel] bolum.gorevlisi role bulundu: {gorevliRole != null}, RoleId: {gorevliRole?.Id}");
                    
                    if (gorevliRole != null)
                    {
                        // Kullanıcının zaten bu role'ü var mı kontrol et
                        var existingUserRole = _userOperationClaimDal.Get(uoc => 
                            uoc.UserId == akademikPersonel.UserId && 
                            uoc.OperationClaimId == gorevliRole.Id
                        );
                        Console.WriteLine($"[BolumAkademikPersonel] Kullanıcıda mevcut role var mı: {existingUserRole != null}");
                        
                        // Eğer role yoksa ekle
                        if (existingUserRole == null)
                        {
                            var userOperationClaim = new UserOperationClaim
                            {
                                UserId = akademikPersonel.UserId,
                                OperationClaimId = gorevliRole.Id
                            };
                            _userOperationClaimDal.Add(userOperationClaim);
                            Console.WriteLine($"[BolumAkademikPersonel] ✅ bolum.gorevlisi role'ü eklendi - UserId: {akademikPersonel.UserId}");
                        }
                        else
                        {
                            Console.WriteLine($"[BolumAkademikPersonel] ⚠️ Kullanıcı zaten bolum.gorevlisi role'üne sahip");
                        }
                    }
                }

                return new SuccessResult("Akademik personel bölüme atandı ve gerekli yetkiler verildi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BolumAkademikPersonel] ❌ HATA: {ex.Message}");
                Console.WriteLine($"[BolumAkademikPersonel] Stack Trace: {ex.StackTrace}");
                return new ErrorResult($"Atama işlemi sırasında bir hata oluştu: {ex.Message}");
            }
        }

        public IResult Delete(BolumAkademikPersoneller bolumAkademikPersoneller)
        {
            try
            {
                // ✅ YENİ: Silmeden önce kontroller yap
                var akademikPersonel = _akademikPersonelDal.Get(ap => ap.Id == bolumAkademikPersoneller.AkademikPersonelId);
                
                if (akademikPersonel != null && akademikPersonel.UserId > 0)
                {
                    // Bu akademik personelin diğer aktif bölüm atamalarını kontrol et
                    var otherAssignments = _bolumAkademikPersonellerDal.GetAll(bap => 
                        bap.AkademikPersonelId == bolumAkademikPersoneller.AkademikPersonelId && 
                        bap.Id != bolumAkademikPersoneller.Id &&
                        bap.Status == true
                    );
                    
                    // Eğer başka aktif bölüm ataması yoksa "bolum.gorevlisi" role'ünü kaldır
                    if (otherAssignments.Count == 0)
                    {
                        var gorevliRole = _operationClaimDal.Get(r => r.Name == "bolum.gorevlisi");
                        
                        if (gorevliRole != null)
                        {
                            var userOperationClaim = _userOperationClaimDal.Get(uoc => 
                                uoc.UserId == akademikPersonel.UserId && 
                                uoc.OperationClaimId == gorevliRole.Id
                            );
                            
                            if (userOperationClaim != null)
                            {
                                _userOperationClaimDal.Delete(userOperationClaim);
                            }
                        }
                    }
                }

                // Atamayı sil
                _bolumAkademikPersonellerDal.Delete(bolumAkademikPersoneller);
                return new SuccessResult("Bölüm ataması kaldırıldı ve yetkiler güncellendi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Silme işlemi sırasında bir hata oluştu: {ex.Message}");
            }
        }

        public IDataResult<BolumAkademikPersoneller> GetById(int id)
        {
            var result = _bolumAkademikPersonellerDal.Get(q=>q.Id == id);
            return new SuccessDataResult<BolumAkademikPersoneller>(result);
        }

        public IDataResult<List<BolumAkademikPersoneller>> GetAll()
        {
            var result = _bolumAkademikPersonellerDal.GetAll();
            return new SuccessDataResult<List<BolumAkademikPersoneller>>(result);
        }

        public IResult Update(BolumAkademikPersoneller bolumAkademikPersoneller)
        {
            _bolumAkademikPersonellerDal.Update(bolumAkademikPersoneller);
            return new SuccessResult();
        }

        public IDataResult<BolumAkademikPersonelListDTO> GetAkademikPersonellerByBolumId(int bolumId)
        {
            var result = _bolumAkademikPersonellerDal.GetAkademikPersonellerByBolumId(bolumId);
            if (result == null)
                return new ErrorDataResult<BolumAkademikPersonelListDTO>("Bölüm bulunamadı.");

            return new SuccessDataResult<BolumAkademikPersonelListDTO>(result, "Bölüme atanmış akademik personeller başarıyla getirildi.");
        }

        public IDataResult<AkademikPersonelBolumListDTO> GetBolumlerByAkademikPersonelId(int akademikPersonelId)
        {
            var result = _bolumAkademikPersonellerDal.GetBolumlerByAkademikPersonelId(akademikPersonelId);
            if (result == null)
                return new ErrorDataResult<AkademikPersonelBolumListDTO>("Akademik personel bulunamadı.");

            return new SuccessDataResult<AkademikPersonelBolumListDTO>(result, "Akademik personelin atandığı bölümler başarıyla getirildi.");
        }
    }
}
