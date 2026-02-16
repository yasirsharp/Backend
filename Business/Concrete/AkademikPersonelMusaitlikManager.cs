using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entity.Concrete;
using Entity.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business.Concrete
{
    /// <summary>
    /// Akademik personel müsaitlik yönetimi iş mantığı katmanı
    /// Sınav gözetmen ataması için müsaitlik kontrolü sağlar
    /// </summary>
    public class AkademikPersonelMusaitlikManager : IAkademikPersonelMusaitlikService
    {
        private readonly IAkademikPersonelMusaitlikDal _musaitlikDal;

        public AkademikPersonelMusaitlikManager(IAkademikPersonelMusaitlikDal musaitlikDal)
        {
            _musaitlikDal = musaitlikDal;
        }

        #region CRUD İşlemleri

        /// <summary>
        /// Yeni müsaitlik kaydı ekler (çakışma kontrolü ile)
        /// </summary>
        public async Task<IResult> AddAsync(MusaitlikCreateDto dto)
        {
            // Saat validasyonu
            if (dto.BaslangicSaati.HasValue && dto.BitisSaati.HasValue
                && dto.BaslangicSaati.Value >= dto.BitisSaati.Value)
            {
                return new ErrorResult("Başlangıç saati bitiş saatinden önce olmalıdır.");
            }

            // Tarih validasyonu
            if (dto.BitisTarihi.HasValue && dto.BaslangicTarihi > dto.BitisTarihi.Value)
            {
                return new ErrorResult("Başlangıç tarihi bitiş tarihinden sonra olamaz.");
            }

            // Çakışma kontrolü
            var cakismaVar = await _musaitlikDal.HasOverlapAsync(
                dto.AkademikPersonelId,
                dto.BaslangicTarihi,
                dto.BitisTarihi,
                dto.BaslangicSaati,
                dto.BitisSaati,
                dto.TekrarTipi,
                excludeId: null);

            if (cakismaVar)
            {
                return new ErrorResult(Messages.MusaitlikCakisma);
            }

            // DTO'dan entity oluştur
            var musaitlik = new AkademikPersonelMusaitlik
            {
                AkademikPersonelId = dto.AkademikPersonelId,
                BaslangicTarihi = dto.BaslangicTarihi,
                BitisTarihi = dto.BitisTarihi,
                BaslangicSaati = dto.BaslangicSaati,
                BitisSaati = dto.BitisSaati,
                TekrarTipi = dto.TekrarTipi,
                IsMusait = dto.IsMusait,
                Neden = dto.Neden,
                Aciklama = dto.Aciklama,
                CreatedDate = DateTime.Now,
                Status = true
            };

            // Tekrar tipine göre TekrarGunu'nu otomatik ayarla
            SetTekrarGunu(musaitlik);

            await _musaitlikDal.AddAsync(musaitlik);
            return new SuccessResult(Messages.MusaitlikAdded);
        }

        /// <summary>
        /// Müsaitlik kaydını günceller
        /// </summary>
        public async Task<IResult> UpdateAsync(MusaitlikUpdateDto dto)
        {
            var existing = await _musaitlikDal.GetAsync(m => m.Id == dto.Id && m.Status);
            if (existing == null)
                return new ErrorResult(Messages.MusaitlikNotFound);

            // Saat validasyonu
            if (dto.BaslangicSaati.HasValue && dto.BitisSaati.HasValue
                && dto.BaslangicSaati.Value >= dto.BitisSaati.Value)
            {
                return new ErrorResult("Başlangıç saati bitiş saatinden önce olmalıdır.");
            }

            // Çakışma kontrolü (kendi kaydını hariç tut)
            var cakismaVar = await _musaitlikDal.HasOverlapAsync(
                dto.AkademikPersonelId,
                dto.BaslangicTarihi,
                dto.BitisTarihi,
                dto.BaslangicSaati,
                dto.BitisSaati,
                dto.TekrarTipi,
                excludeId: dto.Id);

            if (cakismaVar)
            {
                return new ErrorResult(Messages.MusaitlikCakisma);
            }

            // Mevcut entity'yi güncelle
            existing.AkademikPersonelId = dto.AkademikPersonelId;
            existing.BaslangicTarihi = dto.BaslangicTarihi;
            existing.BitisTarihi = dto.BitisTarihi;
            existing.BaslangicSaati = dto.BaslangicSaati;
            existing.BitisSaati = dto.BitisSaati;
            existing.TekrarTipi = dto.TekrarTipi;
            existing.IsMusait = dto.IsMusait;
            existing.Neden = dto.Neden;
            existing.Aciklama = dto.Aciklama;
            existing.UpdatedDate = DateTime.Now;

            // Tekrar tipine göre TekrarGunu'nu güncelle
            SetTekrarGunu(existing);

            await _musaitlikDal.UpdateAsync(existing);
            return new SuccessResult(Messages.MusaitlikUpdated);
        }

        /// <summary>
        /// Müsaitlik kaydını soft delete yapar
        /// </summary>
        public async Task<IResult> DeleteAsync(int id)
        {
            var musaitlik = await _musaitlikDal.GetAsync(m => m.Id == id);
            if (musaitlik == null)
                return new ErrorResult(Messages.MusaitlikNotFound);

            // Soft delete
            musaitlik.Status = false;
            musaitlik.UpdatedDate = DateTime.Now;

            await _musaitlikDal.UpdateAsync(musaitlik);
            return new SuccessResult(Messages.MusaitlikDeleted);
        }

        /// <summary>
        /// Toplu silme (birden fazla kaydı tek seferde sil)
        /// </summary>
        public async Task<IResult> DeleteBatchAsync(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return new ErrorResult("Silinecek kayıt belirtilmedi.");

            var deletedCount = await _musaitlikDal.DeleteBatchAsync(ids);
            return new SuccessResult($"{deletedCount} müsaitlik kaydı silindi.");
        }

        /// <summary>
        /// ID'ye göre müsaitlik kaydını getirir
        /// </summary>
        public IDataResult<MusaitlikResponseDto> GetById(int id)
        {
            var musaitlik = _musaitlikDal.Get(m => m.Id == id && m.Status);
            if (musaitlik == null)
                return new ErrorDataResult<MusaitlikResponseDto>(Messages.MusaitlikNotFound);

            return new SuccessDataResult<MusaitlikResponseDto>(MapToResponseDto(musaitlik));
        }

        #endregion

        #region Sorgulama İşlemleri

        /// <summary>
        /// Akademik personelin tüm aktif müsaitlik kayıtlarını getirir
        /// </summary>
        public async Task<IDataResult<List<MusaitlikResponseDto>>> GetByPersonelIdAsync(int akademikPersonelId)
        {
            var kayitlar = await _musaitlikDal.GetByAkademikPersonelIdAsync(akademikPersonelId);
            var dtoList = kayitlar.Select(MapToResponseDto).ToList();
            return new SuccessDataResult<List<MusaitlikResponseDto>>(dtoList);
        }

        /// <summary>
        /// Akademik personelin belirli ay için müsaitlik takvimini getirir
        /// </summary>
        public async Task<IDataResult<List<MusaitlikResponseDto>>> GetTakvimAsync(
            int akademikPersonelId,
            int yil,
            int ay)
        {
            var ayBaslangic = new DateTime(yil, ay, 1);
            var ayBitis = ayBaslangic.AddMonths(1).AddDays(-1);

            var kayitlar = await _musaitlikDal.GetByDateRangeAsync(akademikPersonelId, ayBaslangic, ayBitis);
            var dtoList = kayitlar.Select(MapToResponseDto).ToList();
            return new SuccessDataResult<List<MusaitlikResponseDto>>(dtoList);
        }

        /// <summary>
        /// Belirli tarih aralığındaki müsaitlik kayıtlarını getirir
        /// </summary>
        public async Task<IDataResult<List<MusaitlikResponseDto>>> GetByDateRangeAsync(
            int akademikPersonelId,
            DateTime baslangic,
            DateTime bitis)
        {
            var kayitlar = await _musaitlikDal.GetByDateRangeAsync(akademikPersonelId, baslangic, bitis);
            var dtoList = kayitlar.Select(MapToResponseDto).ToList();
            return new SuccessDataResult<List<MusaitlikResponseDto>>(dtoList);
        }

        #endregion

        #region Gözetmen Atama İşlemleri

        /// <summary>
        /// Belirli tarih ve saatte müsait olan personelleri getirir
        /// </summary>
        public async Task<IDataResult<List<AkademikPersonel>>> GetMusaitPersonellerAsync(
            DateTime tarih,
            TimeSpan? baslangicSaati,
            TimeSpan? bitisSaati)
        {
            var musaitPersoneller = await _musaitlikDal.GetMusaitPersonellerAsync(tarih, baslangicSaati, bitisSaati);
            return new SuccessDataResult<List<AkademikPersonel>>(musaitPersoneller);
        }

        /// <summary>
        /// Belirli tarih ve saatte meşgul olan personel ID'lerini getirir
        /// </summary>
        public async Task<IDataResult<List<int>>> GetMesgulPersonelIdlerAsync(
            DateTime tarih,
            TimeSpan? baslangicSaati,
            TimeSpan? bitisSaati)
        {
            var mesgulIdler = await _musaitlikDal.GetMesgulPersonelIdlerAsync(tarih, baslangicSaati, bitisSaati);
            return new SuccessDataResult<List<int>>(mesgulIdler);
        }

        /// <summary>
        /// Personelin belirli tarih ve saatte meşgul olup olmadığını kontrol eder
        /// </summary>
        public async Task<IDataResult<bool>> IsMesgulAsync(
            int akademikPersonelId,
            DateTime tarih,
            TimeSpan? baslangicSaati,
            TimeSpan? bitisSaati)
        {
            var isMesgul = await _musaitlikDal.IsMesgulAsync(
                akademikPersonelId, tarih, baslangicSaati, bitisSaati);

            var message = isMesgul ? Messages.PersonelMesgul : Messages.PersonelMusait;
            return new SuccessDataResult<bool>(isMesgul, message);
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Tekrar tipine göre TekrarGunu değerini otomatik ayarlar
        /// </summary>
        private void SetTekrarGunu(AkademikPersonelMusaitlik musaitlik)
        {
            switch (musaitlik.TekrarTipi)
            {
                case TekrarTipiEnum.Tekil:
                    musaitlik.TekrarGunu = null;
                    break;

                case TekrarTipiEnum.Haftalik:
                    musaitlik.TekrarGunu = (int)musaitlik.BaslangicTarihi.DayOfWeek;
                    break;

                case TekrarTipiEnum.Aylik:
                    musaitlik.TekrarGunu = musaitlik.BaslangicTarihi.Day;
                    break;
            }
        }

        /// <summary>
        /// Entity'yi ResponseDto'ya dönüştürür
        /// </summary>
        private MusaitlikResponseDto MapToResponseDto(AkademikPersonelMusaitlik entity)
        {
            return new MusaitlikResponseDto
            {
                Id = entity.Id,
                AkademikPersonelId = entity.AkademikPersonelId,
                BaslangicTarihi = entity.BaslangicTarihi,
                BitisTarihi = entity.BitisTarihi,
                BaslangicSaati = entity.BaslangicSaati,
                BitisSaati = entity.BitisSaati,
                TekrarTipi = (int)entity.TekrarTipi,
                TekrarGunu = entity.TekrarGunu,
                IsMusait = entity.IsMusait,
                Neden = entity.Neden,
                Aciklama = entity.Aciklama,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate
            };
        }

        #endregion
    }
}
