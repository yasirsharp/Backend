using Core.DataAccess;
using Core.Entities.Concrete;

namespace DataAccess.Abstract
{
    public interface IUserDal:IEntityRepository<User>
    {
        public List<OperationClaim> GetClaims(User user);
        public List<int> GetBolumIds(int userId);
    }
}
