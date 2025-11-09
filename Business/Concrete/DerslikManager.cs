using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using DataAccess.Abstract;
using DataAccess.Concrete;
using DataAccess.Concrete.EntityFramework;
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
    public class DerslikManager : IDerslikService
    {
        private IDerslikDal _derslikDal;
        private IDerslikBolumDal _derslikBolumDal;

        public DerslikManager(IDerslikDal derslikDal, IDerslikBolumDal derslikBolumDal)
        {
            _derslikDal = derslikDal;
            _derslikBolumDal = derslikBolumDal;
        }
        IResult IDerslikService.Add(Derslik derslik)
        {
            _derslikDal.Add(derslik);
            return new SuccessResult(Messages.DerslikAdded);
        }

        IResult IDerslikService.Delete(Derslik derslik)
        {
            _derslikDal.Delete(derslik);
            return new SuccessResult(Messages.DerslikDeleted);
        }

        IDataResult<Derslik> IDerslikService.GetById(int derslikId)
        {
            var result = _derslikDal.Get(q=>q.Id == derslikId);
            return new SuccessDataResult<Derslik>(result);
        }

        IDataResult<List<Derslik>> IDerslikService.GetList()
        {
            var result = _derslikDal.GetAll();
            return new SuccessDataResult<List<Derslik>>(result, $"{result.Count} tane sonuç bulundu.");
        }

        IResult IDerslikService.Update(Derslik derslik)
        {
            _derslikDal.Update(derslik);
            return new SuccessResult(Messages.DerslikUpdated);
        }

        public IResult AddDerslikWithBolumler(DerslikEkleDTO derslikEkleDto)
        {
            // 1. Önce dersliği ekle
            var derslik = new Derslik
            {
                Ad = derslikEkleDto.Ad,
                Kapasite = derslikEkleDto.Kapasite,
                Status = derslikEkleDto.Status,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            _derslikDal.Add(derslik);

            // 2. Eğer ortak derslik değilse, seçilen bölümler için DerslikBolum kayıtları oluştur
            if (!derslikEkleDto.OrtakDerslik && derslikEkleDto.BolumIds != null && derslikEkleDto.BolumIds.Count > 0)
            {
                foreach (var bolumId in derslikEkleDto.BolumIds)
                {
                    var derslikBolum = new DerslikBolum
                    {
                        DerslikId = derslik.Id,
                        BolumId = bolumId,
                        CreatedDate = DateTime.Now,
                        Status = true,
                        UpdatedDate = DateTime.Now
                    };
                    _derslikBolumDal.Add(derslikBolum);
                }
                return new SuccessResult($"Derslik eklendi ve {derslikEkleDto.BolumIds.Count} bölümle ilişkilendirildi.");
            }

            return new SuccessResult("Ortak derslik başarıyla eklendi.");
        }

        public IDataResult<List<DerslikWithBolumlerDTO>> GetAllWithBolumler()
        {
            var derslikler = _derslikDal.GetAll();
            var result = new List<DerslikWithBolumlerDTO>();

            foreach (var derslik in derslikler)
            {
                var derslikBolumler = _derslikBolumDal.GetByDerslikId(derslik.Id);
                
                var derslikDto = new DerslikWithBolumlerDTO
                {
                    DerslikId = derslik.Id,
                    DerslikAd = derslik.Ad,
                    Kapasite = derslik.Kapasite,
                    OrtakDerslik = derslikBolumler.Count == 0, // Eğer hiç bölüm ilişkisi yoksa ortak derslik
                    Bolumler = new List<BolumInfo>(),
                    Status = derslik.Status
                };

                if (derslikBolumler.Count > 0)
                {
                    using (var context = new DuzceUniversiteContext())
                    {
                        foreach (var derslikBolum in derslikBolumler)
                        {
                            var bolum = context.Bolum.FirstOrDefault(b => b.Id == derslikBolum.BolumId);
                            if (bolum != null)
                            {
                                derslikDto.Bolumler.Add(new BolumInfo
                                {
                                    BolumId = bolum.Id,
                                    BolumAd = bolum.Ad
                                });
                            }
                        }
                    }
                }

                result.Add(derslikDto);
            }

            return new SuccessDataResult<List<DerslikWithBolumlerDTO>>(result, $"{result.Count} derslik bulundu.");
        }

        public IResult UpdateDerslikWithBolumler(DerslikGuncelleDTO derslikGuncelleDto)
        {
            // 1. Önce dersliği güncelle
            var derslik = new Derslik
            {
                Id = derslikGuncelleDto.Id,
                Ad = derslikGuncelleDto.Ad,
                Kapasite = derslikGuncelleDto.Kapasite,
                Status = derslikGuncelleDto.Status,
                UpdatedDate = DateTime.Now
            };
            _derslikDal.Update(derslik);

            // 2. Mevcut DerslikBolum kayıtlarını sil
            var mevcutDerslikBolumler = _derslikBolumDal.GetByDerslikId(derslikGuncelleDto.Id);
            foreach (var derslikBolum in mevcutDerslikBolumler)
            {
                _derslikBolumDal.Delete(derslikBolum);
            }

            // 3. Eğer ortak derslik değilse, yeni bölüm ilişkilerini oluştur
            if (derslikGuncelleDto.BolumIds != null && derslikGuncelleDto.BolumIds.Count > 0)
            {
                foreach (var bolumId in derslikGuncelleDto.BolumIds)
                {
                    var derslikBolum = new DerslikBolum
                    {
                        DerslikId = derslikGuncelleDto.Id,
                        BolumId = bolumId,
                        CreatedDate = DateTime.Now,
                        UpdatedDate =DateTime.Now,
                        Status = true
                    };
                    _derslikBolumDal.Add(derslikBolum);
                }
                return new SuccessResult($"Derslik güncellendi ve {derslikGuncelleDto.BolumIds.Count} bölümle ilişkilendirildi.");
            }

            return new SuccessResult("Derslik ortak derslik olarak güncellendi.");
        }

        public IDataResult<PagedResult<Derslik>> GetPagedList(PaginationParams paginationParams)
        {
            // Arama terimi varsa filtrele (Ad içinde ara)
            if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
            {
                var pagedResult = _derslikDal.GetPaged(
                    paginationParams,
                    d => d.Ad.Contains(paginationParams.SearchTerm)
                );
                return new SuccessDataResult<PagedResult<Derslik>>(
                    pagedResult,
                    $"{pagedResult.TotalCount} derslik bulundu."
                );
            }

            var result = _derslikDal.GetPaged(paginationParams);
            return new SuccessDataResult<PagedResult<Derslik>>(
                result,
                $"{result.TotalCount} derslik bulundu."
            );
        }

        public IResult RemoveDerslikFromBolum(int derslikId, int bolumId)
        {
            // Derslik-Bölüm ilişkisini bul
            var derslikBolum = _derslikBolumDal.Get(db => db.DerslikId == derslikId && db.BolumId == bolumId);
            
            if (derslikBolum == null)
            {
                return new ErrorResult("Bu derslik-bölüm ilişkisi bulunamadı.");
            }

            // Sadece ilişkiyi sil
            _derslikBolumDal.Delete(derslikBolum);

            // Dersliğin başka bölümlerle ilişkisi var mı kontrol et
            var remainingRelations = _derslikBolumDal.GetAll(db => db.DerslikId == derslikId);
            
            if (remainingRelations.Count == 0)
            {
                return new SuccessResult("Derslik bu bölümden kaldırıldı. Artık ortak derslik olarak işaretlendi.");
            }

            return new SuccessResult($"Derslik bu bölümden kaldırıldı. Hala {remainingRelations.Count} bölümle ilişkili.");
        }

        public IDataResult<List<DerslikWithBolumlerDTO>> GetDersliklerByBolumId(int bolumId)
        {
            // bolumId = 0 ise ortak derslikleri getir
            if (bolumId == 0)
            {
                return GetOrtakDerslikler();
            }

            // Belirtilen bölüme ait derslikleri getir
            var derslikBolumler = _derslikBolumDal.GetAll(db => db.BolumId == bolumId);
            var derslikIds = derslikBolumler.Select(db => db.DerslikId).ToList();
            var derslikler = _derslikDal.GetAll(d => derslikIds.Contains(d.Id));

            var result = new List<DerslikWithBolumlerDTO>();

            foreach (var derslik in derslikler)
            {
                var allDerslikBolumler = _derslikBolumDal.GetByDerslikId(derslik.Id);
                
                var derslikDto = new DerslikWithBolumlerDTO
                {
                    DerslikId = derslik.Id,
                    DerslikAd = derslik.Ad,
                    Kapasite = derslik.Kapasite,
                    OrtakDerslik = allDerslikBolumler.Count > 1,
                    Bolumler = new List<BolumInfo>(),
                    Status = derslik.Status
                };

                using (var context = new DuzceUniversiteContext())
                {
                    foreach (var derslikBolum in allDerslikBolumler)
                    {
                        var bolum = context.Bolum.FirstOrDefault(b => b.Id == derslikBolum.BolumId);
                        if (bolum != null)
                        {
                            derslikDto.Bolumler.Add(new BolumInfo
                            {
                                BolumId = bolum.Id,
                                BolumAd = bolum.Ad
                            });
                        }
                    }
                }

                result.Add(derslikDto);
            }

            return new SuccessDataResult<List<DerslikWithBolumlerDTO>>(result, $"{result.Count} derslik bulundu.");
        }

        public IDataResult<List<DerslikWithBolumlerDTO>> GetOrtakDerslikler()
        {
            // Hiçbir bölümle ilişkili olmayan derslikleri getir
            var allDerslikler = _derslikDal.GetAll();
            var result = new List<DerslikWithBolumlerDTO>();

            foreach (var derslik in allDerslikler)
            {
                var derslikBolumler = _derslikBolumDal.GetByDerslikId(derslik.Id);
                
                // Hiçbir bölümle ilişkili değilse ortak derslik
                if (derslikBolumler.Count == 0)
                {
                    var derslikDto = new DerslikWithBolumlerDTO
                    {
                        DerslikId = derslik.Id,
                        DerslikAd = derslik.Ad,
                        Kapasite = derslik.Kapasite,
                        OrtakDerslik = true,
                        Bolumler = new List<BolumInfo>(),
                        Status = derslik.Status
                    };

                    result.Add(derslikDto);
                }
            }

            return new SuccessDataResult<List<DerslikWithBolumlerDTO>>(result, $"{result.Count} ortak derslik bulundu.");
        }

        public IDataResult<List<DerslikWithBolumlerDTO>> GetDersliklerByBolumIds(int[] bolumIds)
        {
            if (bolumIds == null || bolumIds.Length == 0)
            {
                return new ErrorDataResult<List<DerslikWithBolumlerDTO>>("Bölüm ID'leri boş olamaz.");
            }

            // Belirtilen bölümlere ait derslikleri getir (ortak derslikler hariç)
            var derslikBolumler = _derslikBolumDal.GetAll(db => bolumIds.Contains(db.BolumId));
            var derslikIds = derslikBolumler.Select(db => db.DerslikId).Distinct().ToList();
            var derslikler = _derslikDal.GetAll(d => derslikIds.Contains(d.Id));

            var result = new List<DerslikWithBolumlerDTO>();

            foreach (var derslik in derslikler)
            {
                var allDerslikBolumler = _derslikBolumDal.GetByDerslikId(derslik.Id);
                
                // Tüm bölümlere ait derslikleri al (ortak derslikler hariç tutulacak - zaten junction table'da yoklar)
                var derslikDto = new DerslikWithBolumlerDTO
                {
                    DerslikId = derslik.Id,
                    DerslikAd = derslik.Ad,
                    Kapasite = derslik.Kapasite,
                    OrtakDerslik = false,
                    Bolumler = new List<BolumInfo>(),
                    Status = derslik.Status
                };

                using (var context = new DuzceUniversiteContext())
                {
                    foreach (var derslikBolum in allDerslikBolumler)
                    {
                        var bolum = context.Bolum.FirstOrDefault(b => b.Id == derslikBolum.BolumId);
                        if (bolum != null)
                        {
                            derslikDto.Bolumler.Add(new BolumInfo
                            {
                                BolumId = bolum.Id,
                                BolumAd = bolum.Ad
                            });
                        }
                    }
                }

                result.Add(derslikDto);
            }

            return new SuccessDataResult<List<DerslikWithBolumlerDTO>>(result, $"{result.Count} derslik bulundu.");
        }
    }
}
