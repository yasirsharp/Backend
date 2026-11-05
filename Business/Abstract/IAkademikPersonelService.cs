using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;
using Entity.DTOs;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IAkademikPersonelService
    {
        IDataResult<List<AkademikPersonel>> GetList(Expression<Func<AkademikPersonel, bool>> filter = null);
        IDataResult<AkademikPersonel> GetById(int akademikPersonelId);
        IDataResult<AkademikPersonel> GetByUserId(int userId);
        /// <summary>Personel, kullanıcı hesabı (geçici parolalı) ve "akademik.personel" rolünü birlikte oluşturur.</summary>
        Task<IDataResult<CreatedAccountDto>> CreateAsync(AkademikPersonelCreateDto dto);
        /// <summary>Her satır ayrı kaydedilir; hatalı satırlar diğerlerini etkilemez.</summary>
        Task<IDataResult<BulkResultDto>> BulkCreateAsync(List<AkademikPersonelCreateDto> rows);
        /// <summary>Bölümde ders veren veya bölüm görevlisi olan personel.</summary>
        IDataResult<List<AkademikPersonel>> GetByBolumId(int bolumId);
        Task<IResult> Update(AkademikPersonel akademikPersonel);
        Task<IResult> Delete(AkademikPersonel akademikPersonel);
        IDataResult<PagedResult<AkademikPersonel>> GetPagedList(PaginationParams paginationParams);
    }
}
