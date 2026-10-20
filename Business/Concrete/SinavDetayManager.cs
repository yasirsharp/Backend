using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.Constants;
using Business.Helpers;
using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using DataAccess.Abstract;
using Entity.Concrete;
using Entity.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class SinavDetayManager : ISinavDetayService
    {
        ISinavDetayDal _sinavDetayDal;
        IAkademikPersonelService _akademikPersonelService;
        IAkademikPersonelMusaitlikDal _musaitlikDal;
        SinavBildirimci _bildirimci;

        public SinavDetayManager(ISinavDetayDal sinavDetayDal, IAkademikPersonelService akademikPersonelService,
            IAkademikPersonelMusaitlikDal musaitlikDal, INotificationDal notificationDal)
        {
            _sinavDetayDal = sinavDetayDal;
            _akademikPersonelService = akademikPersonelService;
            _musaitlikDal = musaitlikDal;
            _bildirimci = new SinavBildirimci(notificationDal);
        }

        /// <summary>Bildirim bilgisi okunamazsa sınav işlemi yine de tamamlanır.</summary>
        private SinavBildirimBilgisiDTO? BildirimBilgisi(int sinavId)
        {
            try
            {
                return _sinavDetayDal.GetBildirimBilgisi(sinavId);
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex, "Sınav bildirim bilgisi okunamadı.");
                return null;
            }
        }
        public IDataResult<List<SinavDetayDTO>> GetByDerslikler(int[] derslikIds)
        {
            try
            {
                var result = _sinavDetayDal.GetByDerslikler(derslikIds);
                return new SuccessDataResult<List<SinavDetayDTO>>(result);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<SinavDetayDTO>>(ExceptionLogger.Log(ex, Messages.SomethingWrong));
            }
        }

        public IDataResult<List<SinavDetayDTO>> GetByDersliklerAndBolum(int[] derslikIds, int bolumId)
        {
            try
            {
                var result = _sinavDetayDal.GetByDersliklerAndBolum(derslikIds, bolumId);
                return new SuccessDataResult<List<SinavDetayDTO>>(result);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<SinavDetayDTO>>(ExceptionLogger.Log(ex, Messages.SomethingWrong));
            }
        }

        public IDataResult<List<SinavDetayDTO>> GetByDersliklerAndAkademikPersonel(int[] derslikIds, int akademikPersonelId)
        {
            try
            {
                var result = _sinavDetayDal.GetByDersliklerAndAkademikPersonel(derslikIds, akademikPersonelId);
                return new SuccessDataResult<List<SinavDetayDTO>>(result);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<SinavDetayDTO>>(ExceptionLogger.Log(ex, Messages.SomethingWrong));
            }
        }

        public IDataResult<List<SinavDetayDTO>> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                var result = _sinavDetayDal.GetSinavDetailsByDateRange(startDate, endDate);
                return new SuccessDataResult<List<SinavDetayDTO>>(result, $"{result.Count} tane sonuç bulundu.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<SinavDetayDTO>>(ExceptionLogger.Log(ex, Messages.SomethingWrong));
            }
        }

        public IDataResult<List<SinavDetayDTO>> GetByDateRangeAndBolum(DateTime startDate, DateTime endDate, int bolumId)
        {
            try
            {
                var result = _sinavDetayDal.GetSinavDetailsByDateRangeAndBolum(startDate, endDate, bolumId);
                return new SuccessDataResult<List<SinavDetayDTO>>(result, $"{result.Count} tane sonuç bulundu.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<SinavDetayDTO>>(ExceptionLogger.Log(ex, Messages.SomethingWrong));
            }
        }

        public IDataResult<List<SinavDetayDTO>> GetByDateRangeAndDerslik(DateTime startDate, DateTime endDate, int derslikId)
        {
            try
            {
                var result = _sinavDetayDal.GetSinavDetailsByDateRangeAndDerslik(startDate, endDate, derslikId);
                return new SuccessDataResult<List<SinavDetayDTO>>(result, $"{result.Count} tane sonuç bulundu.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<SinavDetayDTO>>(ExceptionLogger.Log(ex, Messages.SomethingWrong));
            }
        }

        public IDataResult<List<SinavDetayDTO>> GetByDateRangeAndAkademikPersonel(DateTime startDate, DateTime endDate, int akademikPersonelId)
        {
            try
            {
                var result = _sinavDetayDal.GetSinavDetailsByDateRangeAndAkademikPersonel(startDate, endDate, akademikPersonelId);
                return new SuccessDataResult<List<SinavDetayDTO>>(result, $"{result.Count} tane sonuç bulundu.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<SinavDetayDTO>>(ExceptionLogger.Log(ex, Messages.SomethingWrong));
            }
        }

        public IDataResult<List<SinavDetayDTO>> GetMyExamsForUser(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // UserId'den AkademikPersonel kaydını bul
                var personelResult = _akademikPersonelService.GetByUserId(userId);
                if (!personelResult.Success)
                {
                    return new ErrorDataResult<List<SinavDetayDTO>>(personelResult.Message);
                }

                var personelId = personelResult.Data.Id;

                // Tarih aralığı belirtilmemişse, varsayılan olarak bugünden itibaren +/- 3 ay kullan
                DateTime effectiveStartDate = startDate ?? DateTime.Now.AddMonths(-3);
                DateTime effectiveEndDate = endDate ?? DateTime.Now.AddMonths(3);

                // Personele ait sınavları tarih aralığında getir
                var result = _sinavDetayDal.GetSinavDetailsByDateRangeAndAkademikPersonel(effectiveStartDate, effectiveEndDate, personelId);
                
                if (result == null || result.Count == 0)
                {
                    return new SuccessDataResult<List<SinavDetayDTO>>(new List<SinavDetayDTO>(), Messages.SinavDetayNotFound);
                }

                return new SuccessDataResult<List<SinavDetayDTO>>(result, $"{result.Count} tane sınav bulundu.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<SinavDetayDTO>>(ExceptionLogger.Log(ex, Messages.SomethingWrong));
            }
        }

        public async Task<IResult> Add(SinavKayitDTO sinavKayitDTO)
        {
            try
            {
                if (sinavKayitDTO.Derslikler == null || sinavKayitDTO.Derslikler.Count == 0)
                    return new ErrorResult("En az bir derslik seçilmelidir.");

                List<int> gozetmenIdleri = sinavKayitDTO.Derslikler.Where(d => d.GozetmenId.HasValue).Select(d => d.GozetmenId.Value).ToList();

                // Saat dönüşümleri
                TimeOnly baslangicSaati = TimeOnly.Parse(sinavKayitDTO.SinavBaslangicSaati);
                TimeOnly bitisSaati = TimeOnly.Parse(sinavKayitDTO.SinavBitisSaati);
                if (bitisSaati <= baslangicSaati)
                    return new ErrorResult("Sınav bitiş saati başlangıç saatinden sonra olmalıdır.");

                // Gözetmenlerin müsaitlik kontrolü
                foreach (var gozetmenId in gozetmenIdleri)
                {
                    var isMesgul = await _musaitlikDal.IsMesgulAsync(
                        gozetmenId, sinavKayitDTO.SinavTarihi,
                        baslangicSaati.ToTimeSpan(), bitisSaati.ToTimeSpan());

                    if (isMesgul)
                    {
                        var personel = _akademikPersonelService.GetById(gozetmenId);
                        var personelAd = personel.Success ? $"{personel.Data.Unvan} {personel.Data.Ad}" : $"ID: {gozetmenId}";
                        return new ErrorResult($"Gözetmen '{personelAd}' bu tarih ve saatte meşgul olarak işaretlenmiştir.");
                    }
                }

                // Çakışma kontrolü ve ekleme tek transaction içinde (eşzamanlı isteklerde çifte rezervasyonu önler)
                var conflict = _sinavDetayDal.AddIfNoConflict(sinavKayitDTO, out var createdId);
                if (conflict != null)
                    return new ErrorResult(Messages.SinavCakisma);

                _bildirimci.Eklendi(BildirimBilgisi(createdId));

                return new SuccessResult(Messages.SinavDetayAdded);
            }
            catch (Exception err)
            {
                return new ErrorResult(ExceptionLogger.Log(err, Messages.SomethingWrong));
            }
        }

        public IResult Delete(SinavDetay sinavDetay)
        {
            try
            {
                // Önce kaydın var olduğundan emin ol
                var existingRecord = _sinavDetayDal.Get(s => s.Id == sinavDetay.Id);
                if (existingRecord == null)
                {
                    return new ErrorResult("Silinecek sınav kaydı bulunamadı.");
                }

                var once = BildirimBilgisi(sinavDetay.Id);
                _sinavDetayDal.DeleteWithTransaction(sinavDetay);
                _bildirimci.Silindi(once);

                return new SuccessResult(Messages.SinavDetayDeleted);
            }
            catch (Exception err)
            {
                return new ErrorResult(ExceptionLogger.Log(err, Messages.SomethingWrong));
            }
        }

        public IDataResult<List<SinavDetay>> GetAll()
        {
            var result = _sinavDetayDal.GetAll();
            return new SuccessDataResult<List<SinavDetay>>(result, $"{result.Count} tane sonuç bulundu.");
        }

        public IDataResult<List<SinavDetayDTO>> GetAllDetails()
        {
            return new SuccessDataResult<List<SinavDetayDTO>>(_sinavDetayDal.GetDetails());
        }

        public IDataResult<List<SinavDetayDTO>> GetByAkademikPersonelId(int akademikPersonelId)
        {
            var result = _sinavDetayDal.GetByAkademikPersonelId(akademikPersonelId);
            return new SuccessDataResult<List<SinavDetayDTO>>(result, $"{result.Count} tane sonuç bulundu.");
        }

        public IDataResult<List<SinavDetayDTO>> GetByBolumId(int bolumId)
        {
            return new SuccessDataResult<List<SinavDetayDTO>>(_sinavDetayDal.GetByBolumId(bolumId));
        }

        public IDataResult<List<SinavDetayDTO>> GetByDBAPId(int akademikPersonelId)
        {
            try
            {
                var result = _sinavDetayDal.GetByDBAPId(akademikPersonelId);
                if (result == null || result.Count == 0)
                {
                    return new SuccessDataResult<List<SinavDetayDTO>>(new List<SinavDetayDTO>(), "Bu eşleştirmeye ait sınav kaydı bulunamadı.");
                }
                return new SuccessDataResult<List<SinavDetayDTO>>(result, $"{result.Count} tane sonuç bulundu.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<SinavDetayDTO>>(new List<SinavDetayDTO>(), ExceptionLogger.Log(ex, Messages.SomethingWrong));
            }
        }

        public IDataResult<List<SinavDetayDTO>> GetByDerslikId(int derslikId)
        {
            var result = _sinavDetayDal.GetByDerslikId(derslikId);
            return new SuccessDataResult<List<SinavDetayDTO>>(result, $"{result.Count} tane sonuç bulundu.");
        }

        public IDataResult<SinavDetayDTO> GetById(int sinavDetayId)
        {
            return new SuccessDataResult<SinavDetayDTO>(_sinavDetayDal.GetDetail(sinavDetayId));
        }

        public async Task<IResult> Update(SinavGuncelleDTO sinavGuncelleDTO)
        {
            try
            {
                // Önce kaydın var olduğundan emin ol
                var existingRecord = _sinavDetayDal.Get(s => s.Id == sinavGuncelleDTO.Id);
                if (existingRecord == null)
                {
                    return new ErrorResult("Güncellenecek sınav kaydı bulunamadı.");
                }

                if (sinavGuncelleDTO.Derslikler == null || sinavGuncelleDTO.Derslikler.Count == 0)
                    return new ErrorResult("En az bir derslik seçilmelidir.");

                if (sinavGuncelleDTO.SinavBitisSaati <= sinavGuncelleDTO.SinavBaslangicSaati)
                    return new ErrorResult("Sınav bitiş saati başlangıç saatinden sonra olmalıdır.");

                List<int> gozetmenIdleri = sinavGuncelleDTO.Derslikler.Where(d => d.GozetmenId.HasValue).Select(d => d.GozetmenId.Value).ToList();

                // Gözetmenlerin müsaitlik kontrolü
                foreach (var gozetmenId in gozetmenIdleri)
                {
                    var isMesgul = await _musaitlikDal.IsMesgulAsync(
                        gozetmenId, sinavGuncelleDTO.SinavTarihi,
                        sinavGuncelleDTO.SinavBaslangicSaati.ToTimeSpan(), sinavGuncelleDTO.SinavBitisSaati.ToTimeSpan());

                    if (isMesgul)
                    {
                        var personel = _akademikPersonelService.GetById(gozetmenId);
                        var personelAd = personel.Success ? $"{personel.Data.Unvan} {personel.Data.Ad}" : $"ID: {gozetmenId}";
                        return new ErrorResult($"Gözetmen '{personelAd}' bu tarih ve saatte meşgul olarak işaretlenmiştir.");
                    }
                }

                // Çakışma kontrolü (kendi ID'si hariç) ve güncelleme tek transaction içinde
                var once = BildirimBilgisi(sinavGuncelleDTO.Id);
                var conflict = _sinavDetayDal.UpdateIfNoConflict(sinavGuncelleDTO);
                if (conflict != null)
                    return new ErrorResult(Messages.SinavCakisma);

                _bildirimci.Guncellendi(once, BildirimBilgisi(sinavGuncelleDTO.Id));

                return new SuccessResult(Messages.SinavDetayUpdated);
            }
            catch (Exception err)
            {
                return new ErrorResult(ExceptionLogger.Log(err, Messages.SomethingWrong));
            }
        }

        public IDataResult<PagedResult<SinavDetay>> GetPagedList(PaginationParams paginationParams)
        {
            try
            {
                // SinavDetay has SinavTarihi property for date-based searching if needed
                var pagedResult = _sinavDetayDal.GetPaged(paginationParams, null);
                return new SuccessDataResult<PagedResult<SinavDetay>>(pagedResult, $"Toplam {pagedResult.TotalCount} sınav detayı bulundu.");
            }
            catch (Exception err)
            {
                return new ErrorDataResult<PagedResult<SinavDetay>>(ExceptionLogger.Log(err, Messages.SomethingWrong));
            }
        }
    }
}
