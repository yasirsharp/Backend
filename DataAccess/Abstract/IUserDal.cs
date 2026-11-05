using Core.DataAccess;
using Core.Entities.Concrete;
using Entity.DTOs;

namespace DataAccess.Abstract
{
    public interface IUserDal:IEntityRepository<User>
    {
        public List<OperationClaim> GetClaims(User user);
        public List<int> GetBolumIds(int userId);
        /// <summary>Görevli personelin görebileceği bölümler</summary>
        public List<int> GetOkumaBolumIds(int userId);
        public void SetOkumaBolumIds(int userId, IEnumerable<int> bolumIds);
        /// <summary>Tüm kullanıcılar rol adlarıyla (iki sorgu, kullanıcı başına ayrı istek yok).</summary>
        public List<UserWithRolesDto> GetAllWithRoles();
    }
}
