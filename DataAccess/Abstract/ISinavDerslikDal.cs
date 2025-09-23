using Core.DataAccess;
using Entity.Concrete;

namespace DataAccess.Abstract
{
    public interface ISinavDerslikDal : IEntityRepository<SinavDerslik>
    {
        List<SinavDetay> GetByDerslikId(int derslikId);
        List<SinavDetay> GetByGozetmenId(int GozetmenId);
        List<SinavDerslik> GetBySinavDetayId(int sinavDetayId);
    }
}
