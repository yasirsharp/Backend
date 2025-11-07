using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using DataAccess.Abstract;
using Entity.Concrete;

namespace Business.Concrete
{
    /// <summary>
    /// Ogrenci Business Manager
    /// Öğrenci business logic operasyonlarını yönetir
    /// </summary>
    public class OgrenciManager : IOgrenciService
    {
        private readonly IOgrenciDal _ogrenciDal;

        public OgrenciManager(IOgrenciDal ogrenciDal)
        {
            _ogrenciDal = ogrenciDal;
        }

        public IResult Add(Ogrenci ogrenci)
        {
            // Aynı UserId ile kayıt var mı kontrol et (1-to-1 constraint)
            var existingByUserId = _ogrenciDal.Get(o => o.UserId == ogrenci.UserId);
            if (existingByUserId != null)
            {
                return new ErrorResult($"Bu kullanıcı için zaten bir öğrenci kaydı mevcut.");
            }

            // Aynı OgrenciNo ile kayıt var mı kontrol et
            var existingByOgrenciNo = _ogrenciDal.Get(o => o.OgrenciNo == ogrenci.OgrenciNo);
            if (existingByOgrenciNo != null)
            {
                return new ErrorResult($"Bu öğrenci numarası ({ogrenci.OgrenciNo}) zaten kullanılıyor.");
            }

            ogrenci.CreatedDate = DateTime.Now;
            ogrenci.Status = true; // Default aktif

            _ogrenciDal.Add(ogrenci);
            return new SuccessResult(Messages.OgrenciAdded);
        }

        public IResult Delete(Ogrenci ogrenci)
        {
            _ogrenciDal.Delete(ogrenci);
            return new SuccessResult(Messages.OgrenciDeleted);
        }

        public IDataResult<Ogrenci> GetById(int ogrenciId)
        {
            var ogrenci = _ogrenciDal.Get(o => o.Id == ogrenciId);
            if (ogrenci == null)
            {
                return new ErrorDataResult<Ogrenci>($"ID: {ogrenciId} öğrenci bulunamadı.");
            }
            return new SuccessDataResult<Ogrenci>(ogrenci);
        }

        public IDataResult<Ogrenci> GetByUserId(int userId)
        {
            var ogrenci = _ogrenciDal.Get(o => o.UserId == userId);
            if (ogrenci == null)
            {
                return new ErrorDataResult<Ogrenci>($"UserId: {userId} için öğrenci kaydı bulunamadı.");
            }
            return new SuccessDataResult<Ogrenci>(ogrenci);
        }

        public IDataResult<Ogrenci> GetByOgrenciNo(string ogrenciNo)
        {
            var ogrenci = _ogrenciDal.Get(o => o.OgrenciNo == ogrenciNo);
            if (ogrenci == null)
            {
                return new ErrorDataResult<Ogrenci>($"Öğrenci No: {ogrenciNo} bulunamadı.");
            }
            return new SuccessDataResult<Ogrenci>(ogrenci);
        }

        public IDataResult<List<Ogrenci>> GetList()
        {
            var ogrenciler = _ogrenciDal.GetAll();
            return new SuccessDataResult<List<Ogrenci>>(
                ogrenciler,
                $"{ogrenciler.Count} öğrenci bulundu."
            );
        }

        public IDataResult<PagedResult<Ogrenci>> GetPagedList(PaginationParams paginationParams)
        {
            // Arama terimi varsa filtrele (Ad, Soyad, OgrenciNo)
            if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
            {
                var pagedResult = _ogrenciDal.GetPaged(
                    paginationParams,
                    o => o.Ad.Contains(paginationParams.SearchTerm) ||
                         o.Soyad.Contains(paginationParams.SearchTerm) ||
                         o.OgrenciNo.Contains(paginationParams.SearchTerm)
                );
                return new SuccessDataResult<PagedResult<Ogrenci>>(
                    pagedResult,
                    $"{pagedResult.TotalCount} öğrenci bulundu."
                );
            }

            // Arama terimi yoksa hepsini getir
            var result = _ogrenciDal.GetPaged(paginationParams);
            return new SuccessDataResult<PagedResult<Ogrenci>>(
                result,
                $"{result.TotalCount} öğrenci bulundu."
            );
        }

        public IResult Update(Ogrenci ogrenci)
        {
            // Aynı OgrenciNo ile başka kayıt var mı kontrol et (kendisi hariç)
            var existingByOgrenciNo = _ogrenciDal.Get(
                o => o.OgrenciNo == ogrenci.OgrenciNo && o.Id != ogrenci.Id
            );
            if (existingByOgrenciNo != null)
            {
                return new ErrorResult($"Bu öğrenci numarası ({ogrenci.OgrenciNo}) başka bir kayıt tarafından kullanılıyor.");
            }

            ogrenci.UpdatedDate = DateTime.Now;
            _ogrenciDal.Update(ogrenci);
            return new SuccessResult(Messages.OgrenciUpdated);
        }
    }
}
