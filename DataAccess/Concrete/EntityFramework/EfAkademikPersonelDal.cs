using Core.DataAccess.EntityFramework;
using Core.Entities.Concrete;
using DataAccess.Abstract;
using Entity.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfAkademikPersonelDal
        : EfEntityRepositoryBase<AkademikPersonel, DuzceUniversiteContext>,
          IAkademikPersonelDal
    {
        private readonly DuzceUniversiteContext _context;

        public EfAkademikPersonelDal(DuzceUniversiteContext context)
        {
            _context = context;
        }

        public async Task AddWithUserAsync(
            User user,
            AkademikPersonel akademikPersonel,
            UserOperationClaim userOperationClaim)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                akademikPersonel.UserId = user.Id;
                _context.AkademikPersonel.Add(akademikPersonel);

                userOperationClaim.UserId = user.Id;
                _context.UserOperationClaims.Add(userOperationClaim);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateWithUserAsync(
            User user,
            AkademikPersonel akademikPersonel)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Users.Update(user);
                _context.AkademikPersonel.Update(akademikPersonel);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteWithUserAsync(AkademikPersonel akademikPersonel)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var claims = await _context.UserOperationClaims
                    .Where(x => x.UserId == akademikPersonel.UserId)
                    .ToListAsync();

                _context.UserOperationClaims.RemoveRange(claims);

                var user = await _context.Users.FindAsync(akademikPersonel.UserId);
                if (user != null)
                {
                    user.Status = false; // Soft delete
                    _context.Users.Update(user);
                }

                akademikPersonel.Status = false; // Soft delete
                _context.AkademikPersonel.Update(akademikPersonel);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
