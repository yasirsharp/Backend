using Core.Utilities.Results;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IDersBolumService
    {
        IDataResult<List<DersBolum>> GetList();
        IDataResult<DersBolum> GetById(int dersBolumId);
        IDataResult<List<DersBolum>> GetByDersId(int dersId);
        IDataResult<List<DersBolum>> GetByBolumId(int bolumId);
        IResult Add(DersBolum dersBolum);
        IResult Delete(DersBolum dersBolum);
        IResult Update(DersBolum dersBolum);
        IResult DeleteByDersId(int dersId);
        IResult AddMultiple(List<DersBolum> dersBolumler);
    }
}
