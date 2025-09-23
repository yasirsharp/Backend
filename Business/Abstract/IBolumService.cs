using Core.Utilities.Results;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IBolumService
    {
        IDataResult<List<Bolum>> GetList();
        IDataResult<Bolum> GetById(int bolumId);
        IResult Add(Bolum bolum);
        IResult Delete(Bolum bolum);
        IResult Update(Bolum bolum);
    }
}
