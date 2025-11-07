using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;

namespace Business.Abstract
{
    /// <summary>
    /// Ogrenci Service Interface
    /// Öğrenci business logic operasyonları için interface
    /// </summary>
    public interface IOgrenciService
    {
        /// <summary>
        /// Tüm öğrencileri döndürür
        /// </summary>
        IDataResult<List<Ogrenci>> GetList();

        /// <summary>
        /// ID'ye göre öğrenci getirir
        /// </summary>
        IDataResult<Ogrenci> GetById(int ogrenciId);

        /// <summary>
        /// UserId'ye göre öğrenci getirir (1-to-1 relationship)
        /// </summary>
        IDataResult<Ogrenci> GetByUserId(int userId);

        /// <summary>
        /// Öğrenci numarasına göre öğrenci getirir
        /// </summary>
        IDataResult<Ogrenci> GetByOgrenciNo(string ogrenciNo);

        /// <summary>
        /// Yeni öğrenci ekler
        /// </summary>
        IResult Add(Ogrenci ogrenci);

        /// <summary>
        /// Öğrenci siler
        /// </summary>
        IResult Delete(Ogrenci ogrenci);

        /// <summary>
        /// Öğrenci günceller
        /// </summary>
        IResult Update(Ogrenci ogrenci);

        /// <summary>
        /// Sayfalanmış, sıralanmış ve filtrelenmiş öğrenci listesi döner
        /// </summary>
        IDataResult<PagedResult<Ogrenci>> GetPagedList(PaginationParams paginationParams);
    }
}
