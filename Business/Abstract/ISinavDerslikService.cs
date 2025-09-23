using Core.Utilities.Results;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface ISinavDerslikService
    {
        IDataResult<List<SinavDerslik>> GetAll();
        IDataResult<List<SinavDetay>> GetByDerslikId(int derslikId);
        IDataResult<List<SinavDetay>> GetByGozetmenId(int gozetmenId);
        IDataResult<List<SinavDerslik>> GetBySinavDetayId(int sinavDetayId);
        IDataResult<SinavDerslik> Get(int id);

        IResult Add(SinavDerslik sinavDerslik);
        IResult Delete(SinavDerslik sinavDerslik);
        IResult Update(SinavDerslik sinavDerslik);
    }
}
