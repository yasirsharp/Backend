using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entity.Concrete;
using Entity.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class SinavDetayManager : ISinavDetayService
    {
        ISinavDetayDal _sinavDetayDal;

        public SinavDetayManager(ISinavDetayDal sinavDetayDal)
        {
            _sinavDetayDal = sinavDetayDal;
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
                return new ErrorDataResult<List<SinavDetayDTO>>(ex.Message);
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
                return new ErrorDataResult<List<SinavDetayDTO>>(ex.Message);
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
                return new ErrorDataResult<List<SinavDetayDTO>>(ex.Message);
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
                return new ErrorDataResult<List<SinavDetayDTO>>(ex.Message);
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
                return new ErrorDataResult<List<SinavDetayDTO>>(ex.Message);
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
                return new ErrorDataResult<List<SinavDetayDTO>>(ex.Message);
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
                return new ErrorDataResult<List<SinavDetayDTO>>(ex.Message);
            }
        }

        //[SecuredOperation("super.admin,bolum.baskan")]
        public IResult Add(SinavKayitDTO sinavKayitDTO)
        {
            try
            {
                // Derslik ve gözetmenleri listeye çevir
                List<int> derslikIdleri = sinavKayitDTO.Derslikler.Select(d => d.DerslikId).ToList();
                List<int> gozetmenIdleri = sinavKayitDTO.Derslikler.Where(d => d.GozetmenId.HasValue).Select(d => d.GozetmenId.Value).ToList();

                // Saat dönüşümleri
                TimeOnly baslangicSaati = TimeOnly.Parse(sinavKayitDTO.SinavBaslangicSaati);
                TimeOnly bitisSaati = TimeOnly.Parse(sinavKayitDTO.SinavBitisSaati);

                // Çakışma kontrolü
                var result = _sinavDetayDal.ExistSinav(derslikIdleri, gozetmenIdleri, sinavKayitDTO.DersBolumAkademikPersonelId,
                                                                      baslangicSaati, bitisSaati, sinavKayitDTO.SinavTarihi);

                if (result!=null)
                    return new ErrorResult("Derslik, gözetmen veya akademik personel için çakışan bir sınav bulunmaktadır. Lütfen kontrol ediniz.");

                // Eğer çakışma yoksa, sınavı ekle
                _sinavDetayDal.AddWithTransaction(sinavKayitDTO);
                return new SuccessResult(Messages.SinavDetayAdded);
            }
            catch (Exception err)
            {
                return new ErrorResult(err.Message);
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
                    return new ErrorResult("Güncellenecek sınav kaydı bulunamadı.");
                }

                // Güncellemeyi yap
                _sinavDetayDal.DeleteWithTransaction(sinavDetay);

                return new SuccessResult(Messages.SinavDetayDeleted);
            }
            catch (Exception err)
            {
                return new ErrorResult(err.Message);
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
                return new ErrorDataResult<List<SinavDetayDTO>>(new List<SinavDetayDTO>(), $"Hata: {ex.Message}");
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

        public IResult Update(SinavGuncelleDTO sinavGuncelleDTO)
        {
            try
            {
                // Önce kaydın var olduğundan emin ol
                var existingRecord = _sinavDetayDal.Get(s => s.Id == sinavGuncelleDTO.Id);
                if (existingRecord == null)
                {
                    return new ErrorResult("Güncellenecek sınav kaydı bulunamadı.");
                }

                // Derslik ve gözetmenleri listeye çevir
                List<int> derslikIdleri = sinavGuncelleDTO.Derslikler.Select(d => d.DerslikId).ToList();
                List<int> gozetmenIdleri = sinavGuncelleDTO.Derslikler.Where(d => d.GozetmenId.HasValue).Select(d => d.GozetmenId.Value).ToList();

                // Çakışma kontrolü (kendi ID'si hariç)
                var conflictingExam = _sinavDetayDal.ExistSinav(derslikIdleri, gozetmenIdleri, sinavGuncelleDTO.DersBolumAkademikPersonelId,
                                                                sinavGuncelleDTO.SinavBaslangicSaati, sinavGuncelleDTO.SinavBitisSaati, sinavGuncelleDTO.SinavTarihi);

                // Eğer çakışan sınav varsa ve o sınav kendisi değilse hata döndür
                if (conflictingExam != null && conflictingExam.Id != sinavGuncelleDTO.Id)
                {
                    return new ErrorResult("Derslik, gözetmen veya akademik personel için çakışan bir sınav bulunmaktadır. Lütfen kontrol ediniz.");
                }

                // Güncellemeyi yap
                _sinavDetayDal.UpdateWithTransaction(sinavGuncelleDTO);

                return new SuccessResult(Messages.SinavDetayUpdated);
            }
            catch (Exception err)
            {
                return new ErrorResult(err.Message);
            }
        }
    }
}
