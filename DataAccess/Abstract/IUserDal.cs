using Core.DataAccess;
using Core.Entities.Concrete;
using Entity.DTOs;

namespace DataAccess.Abstract
{
    public interface IUserDal:IEntityRepository<User>
    {
        public List<OperationClaim> GetClaims(User user);
        public List<int> GetBolumIds(int userId);
        /// <summary>Tüm kullanıcılar rol adlarıyla (iki sorgu, kullanıcı başına ayrı istek yok).</summary>
        public List<UserWithRolesDto> GetAllWithRoles();
    }
}
