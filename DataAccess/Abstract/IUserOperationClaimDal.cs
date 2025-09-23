using Core.DataAccess;
using Core.Entities.Concrete;
using Entity.Concrete;
using System.Collections.Generic;

namespace DataAccess.Abstract
{
    public interface IUserOperationClaimDal : IEntityRepository<UserOperationClaim>
    {
        List<OperationClaim> GetClaims(User user);
    }
} 