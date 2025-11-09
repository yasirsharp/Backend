using Business.Abstract;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entity.DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Business.Concrete
{
    /// <summary>
    /// Rapor yönetim servisi
    /// TODO: RDLC rapor entegrasyonu eklenecek
    /// </summary>
    public class ReportManager : IReportService
    {
        private readonly ISinavDetayDal _sinavDetayDal;
        private readonly IDerslikDal _derslikDal;

        public ReportManager(ISinavDetayDal sinavDetayDal, IDerslikDal derslikDal)
        {
            _sinavDetayDal = sinavDetayDal;
            _derslikDal = derslikDal;
        }

        /// <summary>
        /// Resmi sınav takvimi raporu oluştur
        /// TODO: RDLC şablonu ile PDF oluşturulacak
        /// </summary>
        public IDataResult<byte[]> GenerateOfficialSinavReport(DateTime startDate, DateTime endDate, int[] bolumIds)
        {
            try
            {
                // Sınav verilerini al
                var sinavlar = _sinavDetayDal.GetSinavDetailsByDateRange(startDate, endDate);

                // Bölüm filtresi uygula
                if (bolumIds != null && bolumIds.Length > 0)
                {
                    sinavlar = sinavlar.Where(s => bolumIds.Contains(s.BolumId)).ToList();
                }

                if (sinavlar.Count == 0)
                {
                    return new ErrorDataResult<byte[]>("Belirtilen tarih aralığında sınav bulunamadı.");
                }

                // TODO: RDLC rapor oluşturma kodu buraya gelecek
                // Şimdilik placeholder response
                return new ErrorDataResult<byte[]>("RDLC rapor sistemi henüz implement edilmedi. Frontend'deki PDF export kullanılabilir.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<byte[]>($"Rapor oluşturulurken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Gözetmen atama raporu
        /// TODO: RDLC şablonu ile PDF oluşturulacak
        /// </summary>
        public IDataResult<byte[]> GenerateGozetmenAssignmentReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                var sinavlar = _sinavDetayDal.GetSinavDetailsByDateRange(startDate, endDate);

                // Gözetmen ataması olan sınavları filtrele
                var gozetmenAtamalari = sinavlar
                    .SelectMany(s => s.Gozetmenler.Select(g => new
                    {
                        SinavTarihi = s.SinavTarihi,
                        SinavBaslangic = s.SinavBaslangicSaati,
                        SinavBitis = s.SinavBitisSaati,
                        DersAd = s.DersAd,
                        BolumAd = s.BolumAd,
                        GozetmenAd = g.Ad,
                        GozetmenUnvan = g.Unvan
                    }))
                    .OrderBy(x => x.SinavTarihi)
                    .ThenBy(x => x.SinavBaslangic)
                    .ToList();

                if (gozetmenAtamalari.Count == 0)
                {
                    return new ErrorDataResult<byte[]>("Gözetmen ataması bulunamadı.");
                }

                // TODO: RDLC rapor oluşturma kodu buraya gelecek
                return new ErrorDataResult<byte[]>("RDLC rapor sistemi henüz implement edilmedi.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<byte[]>($"Rapor oluşturulurken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Derslik kullanım raporu
        /// TODO: RDLC şablonu ile PDF oluşturulacak
        /// </summary>
        public IDataResult<byte[]> GenerateDerslikUsageReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                var sinavlar = _sinavDetayDal.GetSinavDetailsByDateRange(startDate, endDate);

                // Derslik kullanım istatistikleri
                var derslikKullanimi = sinavlar
                    .SelectMany(s => s.Derslikler.Select(d => new
                    {
                        DerslikId = d.DerslikId,
                        SinavTarihi = s.SinavTarihi,
                        SinavSaati = $"{s.SinavBaslangicSaati} - {s.SinavBitisSaati}",
                        DersAd = s.DersAd,
                        BolumAd = s.BolumAd
                    }))
                    .GroupBy(x => x.DerslikId)
                    .Select(g => new
                    {
                        DerslikId = g.Key,
                        KullanimSayisi = g.Count(),
                        SinavDetaylari = g.ToList()
                    })
                    .OrderByDescending(x => x.KullanimSayisi)
                    .ToList();

                if (derslikKullanimi.Count == 0)
                {
                    return new ErrorDataResult<byte[]>("Derslik kullanım verisi bulunamadı.");
                }

                // TODO: RDLC rapor oluşturma kodu buraya gelecek
                return new ErrorDataResult<byte[]>("RDLC rapor sistemi henüz implement edilmedi.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<byte[]>($"Rapor oluşturulurken hata: {ex.Message}");
            }
        }
    }
}
