using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;
using Entity.DTOs;

namespace Business.Abstract
{
    /// <summary>
    /// Öğrenci işlemleri. Öğrenci eklemek her zaman bir kullanıcı hesabı (geçici parolalı) açar.
    /// </summary>
    public interface IOgrenciService
    {
        IDataResult<List<Ogrenci>> GetList();

        IDataResult<Ogrenci> GetById(int ogrenciId);

        IDataResult<Ogrenci> GetByUserId(int userId);

        IDataResult<Ogrenci> GetByOgrenciNo(string ogrenciNo);

        IDataResult<PagedResult<Ogrenci>> GetPagedList(PaginationParams paginationParams);

        /// <summary>Öğrenci, kullanıcı hesabı ve "ogrenci" rolünü tek transaction içinde oluşturur.</summary>
        Task<IDataResult<CreatedAccountDto>> CreateAsync(OgrenciCreateDto dto, string? password = null);

        /// <summary>Her satır ayrı kaydedilir; hatalı satırlar diğerlerini etkilemez.</summary>
        Task<IDataResult<BulkResultDto>> BulkCreateAsync(List<OgrenciCreateDto> rows);

        Task<IResult> UpdateAsync(OgrenciUpdateDto dto);

        Task<IResult> DeleteAsync(int ogrenciId);
    }
}
