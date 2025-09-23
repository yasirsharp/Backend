using Core.DataAccess;
using Entity.Concrete;
using Entity.DTOs;
using System.Linq.Expressions;

namespace DataAccess.Abstract
{
    public interface IDBAPDal:IEntityRepository<DersBolumAkademikPersonel>
    {
        List<DersBolumAkademikPersonelDTO> GetDetails(Expression<Func<DersBolumAkademikPersonelDTO, bool>> filter = null);
        DersBolumAkademikPersonelDTO GetDetail(int dbapId);
    }
}
