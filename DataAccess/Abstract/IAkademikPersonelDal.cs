using Core.DataAccess;
using Core.Entities.Concrete;
using Entity.Concrete;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface IAkademikPersonelDal : IEntityRepository<AkademikPersonel>
    {
        Task AddWithUserAsync(
            User user,
            AkademikPersonel akademikPersonel,
            UserOperationClaim userOperationClaim);

        Task UpdateWithUserAsync(
            User user,
            AkademikPersonel akademikPersonel);

        Task DeleteWithUserAsync(AkademikPersonel akademikPersonel);
    }
}
