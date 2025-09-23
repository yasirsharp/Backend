using Core.Entities.Concrete;
using System.Security.Claims;

namespace Core.Utilities.Security.JWT
{
    public interface ITokenHelper
    {
        AccessToken CreateToken(User user, List<OperationClaim> operationClaims, List<Claim> bolumIdleri);
    }
}
