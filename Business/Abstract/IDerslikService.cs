using Core.Utilities.Results;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IDerslikService
    {
        IDataResult<List<Derslik>> GetList();
        IDataResult<Derslik> GetById(int derslikId);
        IResult Add(Derslik derslik);
        IResult Delete(Derslik derslik);
        IResult Update(Derslik derslik);
    }
}
