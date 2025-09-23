using Core.Utilities.Results;
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
        IDataResult<DersBolumAkademikPersonelDTO> GetDetail(int dbapId);
        IDataResult<DersBolumAkademikPersonel> GetById(int dbapId);
        IResult Add(DersBolumAkademikPersonel dersBolumAkademikPersonel);
        IResult Delete(DersBolumAkademikPersonel dersBolumAkademikPersonel);
        IResult Update(DersBolumAkademikPersonel dersBolumAkademikPersonel);
    }
}
