using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Entity.Concrete;
using Entity.DTOs;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq.Expressions;

namespace Business.Concrete
{
    public class DersManager : IDersService
    {
        private IDersDal _dersDal;
        IDBAPDal _dBAPDal;
        IDersBolumDal _dersBolumDal;

        public DersManager(IDersDal dersDal, IDBAPDal dBAPDal, IDersBolumDal dersBolumDal)
        {
            _dersDal = dersDal;
            _dBAPDal = dBAPDal;
            _dersBolumDal = dersBolumDal;
        }

        IResult IDersService.Add(Ders ders)
        {
            _dersDal.Add(ders);
            return new SuccessResult(Messages.DersAdded);
        }

        IResult IDersService.Delete(Ders ders)
        {
            var dbap = _dBAPDal.GetDetails(q => q.DersId == ders.Id);
            if (dbap.Count > 0)
            {
                string message = $"{ders.Ad} dersi için {dbap.Count} tane Bölüm-Ders-Akademik Personel Eşleştirmesi bulunmaktadır.\n";
                foreach (var item in dbap)
                {
                    message += $"{item.BolumAd} {item.DersAd} {item.AkademikPersonelAd} {item.Unvan}\n";
                }
                return new ErrorResult(message);
            }
            _dersDal.Delete(ders);
            return new SuccessResult(Messages.DersDeleted);
        }

        IDataResult<Ders> IDersService.GetById(int dersId)
        {
            return new SuccessDataResult<Ders>(_dersDal.Get(q => q.Id == dersId));
        }

        IDataResult<List<Ders>> IDersService.GetList(Expression<Func<Ders, bool>> filter)
        {
            return new SuccessDataResult<List<Ders>>(_dersDal.GetAll(filter), $"{_dersDal.GetAll().Count} tane bulundu.");
        }

        IResult IDersService.Update(Ders ders)
        {
            _dersDal.Update(ders);
            return new SuccessResult(Messages.DersUpdated);
        }

        public IResult AddDersWithBolumler(DersEkleDTO dersEkleDto)
        {
            // 1. Önce dersi ekle
            var ders = new Ders
            {
                Ad = dersEkleDto.Ad
            };
            _dersDal.Add(ders);

            // 2. Eğer ortak ders değilse, seçilen bölümler için DersBolum kayıtları oluştur
            if (!dersEkleDto.OrtakDers && dersEkleDto.BolumIds != null && dersEkleDto.BolumIds.Count > 0)
            {
                foreach (var bolumId in dersEkleDto.BolumIds)
                {
                    var dersBolum = new DersBolum
                    {
                        DersId = ders.Id,
                        BolumId = bolumId,
                        CreatedDate = DateTime.Now
                    };
                    _dersBolumDal.Add(dersBolum);
                }
                return new SuccessResult($"Ders eklendi ve {dersEkleDto.BolumIds.Count} bölümle ilişkilendirildi.");
            }

            return new SuccessResult("Ortak ders başarıyla eklendi.");
        }

        public IDataResult<List<DersWithBolumlerDTO>> GetAllWithBolumler()
        {
            var dersler = _dersDal.GetAll();
            var result = new List<DersWithBolumlerDTO>();

            foreach (var ders in dersler)
            {
                var dersBolumler = _dersBolumDal.GetByDersId(ders.Id);
                
                var dersDto = new DersWithBolumlerDTO
                {
                    DersId = ders.Id,
                    DersAd = ders.Ad,
                    OrtakDers = dersBolumler.Count == 0, // Eğer hiç bölüm ilişkisi yoksa ortak ders
                    Bolumler = new List<BolumInfo>()
                };

                if (dersBolumler.Count > 0)
                {
                    using (var context = new DuzceUniversiteContext())
                    {
                        foreach (var dersBolum in dersBolumler)
                        {
                            var bolum = context.Bolum.FirstOrDefault(b => b.Id == dersBolum.BolumId);
                            if (bolum != null)
                            {
                                dersDto.Bolumler.Add(new BolumInfo
                                {
                                    BolumId = bolum.Id,
                                    BolumAd = bolum.Ad
                                });
                            }
                        }
                    }
                }

                result.Add(dersDto);
            }

            return new SuccessDataResult<List<DersWithBolumlerDTO>>(result, $"{result.Count} ders bulundu.");
        }

        public IResult UpdateDersWithBolumler(DersGuncelleDTO dersGuncelleDto)
        {
            // 1. Önce dersi güncelle
            var ders = new Ders
            {
                Id = dersGuncelleDto.Id,
                Ad = dersGuncelleDto.Ad
            };
            _dersDal.Update(ders);

            // 2. Mevcut DersBolum kayıtlarını sil
            var mevcutDersBolumler = _dersBolumDal.GetByDersId(dersGuncelleDto.Id);
            foreach (var dersBolum in mevcutDersBolumler)
            {
                _dersBolumDal.Delete(dersBolum);
            }

            // 3. Eğer ortak ders değilse, yeni bölüm ilişkilerini oluştur
            if (!dersGuncelleDto.OrtakDers && dersGuncelleDto.BolumIds != null && dersGuncelleDto.BolumIds.Count > 0)
            {
                foreach (var bolumId in dersGuncelleDto.BolumIds)
                {
                    var dersBolum = new DersBolum
                    {
                        DersId = dersGuncelleDto.Id,
                        BolumId = bolumId,
                        CreatedDate = DateTime.Now
                    };
                    _dersBolumDal.Add(dersBolum);
                }
                return new SuccessResult($"Ders güncellendi ve {dersGuncelleDto.BolumIds.Count} bölümle ilişkilendirildi.");
            }

            return new SuccessResult("Ders ortak ders olarak güncellendi.");
        }
    }
}
