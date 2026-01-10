using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;
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
        Task<IResult> Add(AkademikPersonel akademikPersonel);
        Task<IResult> Update(AkademikPersonel akademikPersonel);
        Task<IResult> Delete(AkademikPersonel akademikPersonel);
        IDataResult<PagedResult<AkademikPersonel>> GetPagedList(PaginationParams paginationParams);
    }
}
