using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;
using Entity.DTOs;
using System.Linq.Expressions;

namespace Business.Abstract
{
    public interface IDBAPService
    {
        IDataResult<List<DersBolumAkademikPersonel>> GetAll();
        IDataResult<List<DersBolumAkademikPersonel>> GetByBolumId(int bolumId);
        IDataResult<List<DersBolumAkademikPersonelDTO>> GetAllDetails();
        IDataResult<List<DersBolumAkademikPersonelDTO>> GetDetailsByBolumId(int bolumId);
        IDataResult<List<DersBolumAkademikPersonelDTO>> GetMyCoursesForUser(int userId);
        IDataResult<DersBolumAkademikPersonelDTO> GetDetail(int dbapId);
        IDataResult<DersBolumAkademikPersonel> GetById(int dbapId);
        IResult Add(DersBolumAkademikPersonel dersBolumAkademikPersonel);
        IResult Delete(DersBolumAkademikPersonel dersBolumAkademikPersonel);
        IResult Update(DersBolumAkademikPersonel dersBolumAkademikPersonel);
        IDataResult<PagedResult<DersBolumAkademikPersonelDTO>> GetPagedList(PaginationParams paginationParams);
    }
}
