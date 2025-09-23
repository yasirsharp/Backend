using Core.DataAccess;
using Entity.Concrete;
using Entity.DTOs;

namespace DataAccess.Abstract
{
    public interface IBolumAkademikPersonellerDal : IEntityRepository<BolumAkademikPersoneller>
    {
        BolumAkademikPersonelListDTO GetAkademikPersonellerByBolumId(int bolumId);
        AkademikPersonelBolumListDTO GetBolumlerByAkademikPersonelId(int akademikPersonelId);
    }
}
