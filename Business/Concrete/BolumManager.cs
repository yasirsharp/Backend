using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using DataAccess.Abstract;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class BolumManager : IBolumService
    {
        IBolumDal _bolumDal;
        IDBAPDal _dBAPDal;

        public BolumManager(IBolumDal bolumDal, IDBAPDal dBAPDal)
        {
            _bolumDal = bolumDal;
            _dBAPDal = dBAPDal;
        }

        IResult IBolumService.Add(Bolum bolum)
        {
            _bolumDal.Add(bolum);
            return new SuccessResult(Messages.BolumAdded);
        }

        IResult IBolumService.Delete(Bolum bolum)
        {
            var dbap = _dBAPDal.GetDetails(q => q.BolumId == bolum.Id);
            if (dbap.Count > 0)
            {
                string message = $"{bolum.Ad} dersi için {dbap.Count} tane Bölüm-Ders-Akademik Personel Eşleştirmesi bulunmaktadır.\n";
                foreach (var item in dbap)
                {
                    message += $"{item.BolumAd} {item.DersAd} {item.AkademikPersonelAd} {item.Unvan}\n";
                }
                return new ErrorResult(message);
            }
            _bolumDal.Delete(bolum);
            return new SuccessResult(Messages.BolumDeleted);
        }

        IDataResult<Bolum> IBolumService.GetById(int bolumId)
        {
            return new SuccessDataResult<Bolum>(_bolumDal.Get(p => p.Id == bolumId));
        }

        IDataResult<List<Bolum>> IBolumService.GetByIds(List<int> bolumIds)
        {
            if (bolumIds == null || bolumIds.Count == 0)
            {
                return new ErrorDataResult<List<Bolum>>(new List<Bolum>(), "Bölüm ID listesi boş olamaz.");
            }

            var bolumler = _bolumDal.GetAll(b => bolumIds.Contains(b.Id));
            return new SuccessDataResult<List<Bolum>>(bolumler, $"{bolumler.Count} bölüm bulundu.");
        }

        IDataResult<List<Bolum>> IBolumService.GetList()
        {
            return new SuccessDataResult<List<Bolum>>(_bolumDal.GetAll(), $"{_bolumDal.GetAll().Count} tane bulundu.");
        }

        IResult IBolumService.Update(Bolum bolum)
        {
            _bolumDal.Update(bolum);
            return new SuccessResult(Messages.BolumUpdated);
        }

        public IDataResult<PagedResult<Bolum>> GetPagedList(PaginationParams paginationParams)
        {
            // Arama terimi varsa filtrele
            if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
            {
                var pagedResult = _bolumDal.GetPaged(
                    paginationParams,
                    b => b.Ad.Contains(paginationParams.SearchTerm)
                );
                return new SuccessDataResult<PagedResult<Bolum>>(
                    pagedResult,
                    $"{pagedResult.TotalCount} bölüm bulundu."
                );
            }

            // Arama terimi yoksa hepsini getir
            var result = _bolumDal.GetPaged(paginationParams);
            return new SuccessDataResult<PagedResult<Bolum>>(
                result,
                $"{result.TotalCount} bölüm bulundu."
            );
        }
    }
}
