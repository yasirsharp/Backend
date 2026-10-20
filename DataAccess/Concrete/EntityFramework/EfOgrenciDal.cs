using Core.DataAccess.EntityFramework;
using Core.Entities.Concrete;
using DataAccess.Abstract;
using Entity.Concrete;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Concrete.EntityFramework
{
    /// <summary>
    /// Ogrenci Entity Framework Data Access Layer
    /// Öğrenci veritabanı işlemleri için EF implementasyonu
    /// </summary>
    public class EfOgrenciDal : EfEntityRepositoryBase<Ogrenci, DuzceUniversiteContext>, IOgrenciDal
    {
        // Her çağrı kendi context'ini kullanır: toplu içe aktarmada hatalı bir satır sonraki satırları etkilemez.
        public async Task AddWithUserAsync(User user, Ogrenci ogrenci, string roleName)
        {
            await using var context = new DuzceUniversiteContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var roleId = await context.GetRoleIdAsync(roleName);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            ogrenci.UserId = user.Id;
            context.Ogrenci.Add(ogrenci);
            context.UserOperationClaims.Add(new UserOperationClaim
            {
                UserId = user.Id,
                OperationClaimId = roleId,
                CreatedDate = DateTime.Now,
                Status = true
            });

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task UpdateWithUserAsync(Ogrenci ogrenci)
        {
            await using var context = new DuzceUniversiteContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            context.Ogrenci.Update(ogrenci);

            var user = await context.Users.FindAsync(ogrenci.UserId);
            if (user != null)
            {
                user.FirstName = ogrenci.Ad;
                user.LastName = ogrenci.Soyad;
                // Pasif öğrenci giriş yapamaz
                user.Status = ogrenci.Status;
                user.UpdatedDate = DateTime.Now;
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task DeleteWithUserAsync(Ogrenci ogrenci)
        {
            await using var context = new DuzceUniversiteContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var claims = await context.UserOperationClaims.Where(x => x.UserId == ogrenci.UserId).ToListAsync();
            context.UserOperationClaims.RemoveRange(claims);

            var user = await context.Users.FindAsync(ogrenci.UserId);
            if (user != null)
            {
                user.Status = false; // Kullanıcı silinmez, pasifleştirilir (geçmiş kayıtlar için)
                user.UpdatedDate = DateTime.Now;
            }

            context.Ogrenci.Remove(context.Ogrenci.Attach(ogrenci).Entity);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }
}
