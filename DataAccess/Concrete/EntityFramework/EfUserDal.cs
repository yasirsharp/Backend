using Core.DataAccess.EntityFramework;
using Core.Entities.Concrete;
using DataAccess.Abstract;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfUserDal : EfEntityRepositoryBase<User, DuzceUniversiteContext>, IUserDal
    {
        public List<int> GetBolumIds(int userId)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result =
                    from bap in context.BolumAkademikPersoneller
                    join ap in context.AkademikPersonel on bap.AkademikPersonelId equals ap.Id
                    where ap.UserId == userId
                    select bap.BolumId;
                return result.ToList();
            }
        }

        public List<OperationClaim> GetClaims(User user)
        {
            using (DuzceUniversiteContext context = new DuzceUniversiteContext())
            {
                var result = from operationClaim in context.OperationClaims
                             join userOperationClaim in context.UserOperationClaims
                                 on operationClaim.Id equals userOperationClaim.OperationClaimId
                             where userOperationClaim.UserId == user.Id
                             select new OperationClaim { Id = operationClaim.Id, Name = operationClaim.Name };
                return result.ToList();
            }
        }
    }
}
