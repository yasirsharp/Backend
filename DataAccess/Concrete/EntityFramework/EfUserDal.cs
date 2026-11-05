using Core.DataAccess.EntityFramework;
using Core.Entities.Concrete;
using DataAccess.Abstract;
using Entity.Concrete;
using Entity.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfUserDal : EfEntityRepositoryBase<User, DuzceUniversiteContext>, IUserDal
    {
        public List<int> GetOkumaBolumIds(int userId)
        {
            using var context = new DuzceUniversiteContext();
            return context.KullaniciBolum.Where(x => x.UserId == userId).Select(x => x.BolumId).ToList();
        }

        public void SetOkumaBolumIds(int userId, IEnumerable<int> bolumIds)
        {
            using var context = new DuzceUniversiteContext();
            var wanted = bolumIds.Distinct().ToHashSet();
            var current = context.KullaniciBolum.Where(x => x.UserId == userId).ToList();
            context.KullaniciBolum.RemoveRange(current.Where(x => !wanted.Contains(x.BolumId)));
            foreach (var id in wanted.Where(id => current.All(c => c.BolumId != id)))
                context.KullaniciBolum.Add(new KullaniciBolum { UserId = userId, BolumId = id, CreatedDate = DateTime.Now, Status = true });
            context.SaveChanges();
        }

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

        public List<UserWithRolesDto> GetAllWithRoles()
        {
            using var context = new DuzceUniversiteContext();
            var roles = (from uoc in context.UserOperationClaims
                         join oc in context.OperationClaims on uoc.OperationClaimId equals oc.Id
                         select new { uoc.UserId, oc.Name })
                .ToList()
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Name).OrderBy(n => n).ToList());

            return context.Users
                .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                .ToList()
                .Select(u => new UserWithRolesDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    UserName = u.UserName,
                    Status = u.Status,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate,
                    MustChangePassword = u.MustChangePassword,
                    Roles = roles.TryGetValue(u.Id, out var r) ? r : new List<string>()
                })
                .ToList();
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
