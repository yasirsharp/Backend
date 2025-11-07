using Core.DataAccess.EntityFramework;
using Core.Entities;
using Core.Entities.Concrete;
using Core.Utilities.Results;
using Core.Utilities.Security.Hashing;
using DataAccess.Abstract;
using Entity.Concrete;
using Entity.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfAkademikPersonelDal : EfEntityRepositoryBase<AkademikPersonel, DuzceUniversiteContext>, IAkademikPersonelDal
    {
        /// <summary>
        /// DAL LAYER - Sadece transaction yönetimi yapar, iş kuralları YOK!
        /// Business layer'dan HAZIR nesneler alır ve tek transaction içinde kaydeder.
        /// </summary>
        public async Task AddAkademikPersonelWithUserOperationClaimAsync(
            User user, 
            AkademikPersonel akademikPersonel, 
            UserOperationClaim userOperationClaim)
        {
            using (var context = new DuzceUniversiteContext())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. User ekle (ID oluşsun)
                        context.Users.Add(user);
                        await context.SaveChangesAsync();

                        // 2. AkademikPersonel'e UserId ata ve ekle
                        akademikPersonel.UserId = user.Id;
                        context.AkademikPersonel.Add(akademikPersonel);
                        await context.SaveChangesAsync();

                        // 3. UserOperationClaim'e UserId ata ve ekle
                        userOperationClaim.UserId = user.Id;
                        context.UserOperationClaims.Add(userOperationClaim);
                        await context.SaveChangesAsync();

                        // 4. Commit - Tüm işlemler başarılı
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        // Rollback - Hata durumunda geri al
                        await transaction.RollbackAsync();
                        throw new Exception("Akademik personel kaydı sırasında hata: " + ex.Message, ex);
                    }
                }
            }
        }

        /// <summary>
        /// DEPRECATED - AdminPanel için eski metod (iş kuralları içeriyor - YANLIŞ MİMARİ!)
        /// Geriye dönük uyumluluk için bırakıldı.
        /// </summary>
        [Obsolete("Bu metod iş kuralları içeriyor. AddAkademikPersonelWithUserOperationClaimAsync kullanın.")]
        public async void AddAkademikPersonelWithUserOperationClaim(AkademikPersonel akademikPersonel)
        {
            using (var context = new DuzceUniversiteContext())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Akademik Personel ekleniyor
                        akademikPersonel.UserId = 1;
                        context.AkademikPersonel.Add(akademikPersonel);
                        await context.SaveChangesAsync(); // ID oluşması için kaydediliyor

                        var nameParts = akademikPersonel.Ad.Split(' ');
                        var lastName = nameParts.Length > 1 ? nameParts.Last() : akademikPersonel.Ad;
                        var firstName = akademikPersonel.Ad.Replace(" " + nameParts.Last(), "");

                        string userName = GenerateUserName(akademikPersonel);
                        byte[] passwordHash, passwordSalt;
                        HashingHelper.CreatePasswordHash(userName, out passwordHash, out passwordSalt);

                        // 2. Kullanıcı ekleniyor
                        var user = new User
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            UserName = userName,
                            Email = userName + "@duzce.edu.tr",
                            PasswordHash = passwordHash,
                            PasswordSalt = passwordSalt,
                            Status = true
                        };

                        context.Users.Add(user);
                        await context.SaveChangesAsync(); // Kullanıcı için ID oluşması gerekiyor

                        // 3. Akademik Personel'e UserId atanıyor
                        akademikPersonel.UserId = user.Id;

                        // 4. Akademik Personel güncelleniyor
                        context.AkademikPersonel.Update(akademikPersonel);
                        await context.SaveChangesAsync();

                        // 5. Kullanıcıya rol atanıyor
                        var userOperationClaim = new UserOperationClaim
                        {
                            UserId = user.Id,
                            OperationClaimId = 3 // Akademik personel rolü
                        };

                        await context.UserOperationClaims.AddAsync(userOperationClaim);
                        await context.SaveChangesAsync();

                        // 6. Bütün işlemler başarılı ise transaction'ı commit et
                        await transaction.CommitAsync();
                    }
                    catch (Exception err)
                    {
                        // Hata oluştuğunda transaction'ı geri al
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }


        /// <summary>
        /// YENİ - Doğru mimari: Business layer'dan HAZIR User ve AkademikPersonel nesneleri alır
        /// </summary>
        public async Task UpdateAkademikPersonelWithUserOperationClaimAsync(
            User user, 
            AkademikPersonel akademikPersonel)
        {
            using (var context = new DuzceUniversiteContext())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. AkademikPersonel güncelle
                        context.AkademikPersonel.Update(akademikPersonel);
                        await context.SaveChangesAsync();

                        // 2. User güncelle
                        context.Users.Update(user);
                        await context.SaveChangesAsync();

                        // 3. Commit
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception("Akademik personel güncelleme sırasında hata: " + ex.Message, ex);
                    }
                }
            }
        }

        /// <summary>
        /// DEPRECATED - AdminPanel için eski metod (iş kuralları içeriyor)
        /// </summary>
        [Obsolete("Bu metod iş kuralları içeriyor. UpdateAkademikPersonelWithUserOperationClaimAsync kullanın.")]
        public async void UpdateAkademikPersonelWithUserOperationClaim(AkademikPersonel akademikPersonel)
        {
            using (var context = new DuzceUniversiteContext())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        context.AkademikPersonel.Update(akademikPersonel);

                        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == akademikPersonel.UserId);

                        var nameParts = akademikPersonel.Ad.Split(' ');
                        var lastName = nameParts.Length > 1 ? nameParts.Last() : akademikPersonel.Ad;
                        var firstName = akademikPersonel.Ad.Replace(" " + nameParts.Last(), "");

                        string userName = GenerateUserName(akademikPersonel);
                        byte[] passwordHash, passwordSalt;
                        HashingHelper.CreatePasswordHash(userName, out passwordHash, out passwordSalt);

                        user.FirstName = firstName;
                        user.LastName = lastName;
                        user.UserName = userName;
                        user.Email = userName + "@duzce.edu.tr";
                        user.PasswordHash = passwordHash;
                        user.PasswordSalt = passwordSalt;
                        user.Status = true;

                        context.Users.Update(user);

                        await context.SaveChangesAsync();

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                    }
                }
            }
        }


        /// <summary>
        /// YENİ - Doğru mimari: Sadece transaction yönetimi, iş kuralları YOK
        /// </summary>
        public async Task DeleteAkademikPersonelWithUserOperationClaimAsync(AkademikPersonel akademikPersonel)
        {
            using (var context = new DuzceUniversiteContext())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. UserOperationClaims sil
                        var userOperationClaims = await context.UserOperationClaims
                            .Where(uoc => uoc.UserId == akademikPersonel.UserId)
                            .ToListAsync();
                        context.UserOperationClaims.RemoveRange(userOperationClaims);

                        // 2. AkademikPersonel sil
                        var akademikPersonelToDelete = await context.AkademikPersonel
                            .FirstOrDefaultAsync(ap => ap.Id == akademikPersonel.Id);
                        if (akademikPersonelToDelete != null)
                        {
                            context.AkademikPersonel.Remove(akademikPersonelToDelete);
                        }

                        // 3. User sil
                        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == akademikPersonel.UserId);
                        if (user != null)
                        {
                            context.Users.Remove(user);
                        }

                        await context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception("Akademik personel silme sırasında hata: " + ex.Message, ex);
                    }
                }
            }
        }

        /// <summary>
        /// DEPRECATED - AdminPanel için eski metod
        /// </summary>
        [Obsolete("Bu metod eski. DeleteAkademikPersonelWithUserOperationClaimAsync kullanın.")]
        public async void DeleteAkademikPersonelWithUserOperationClaim(AkademikPersonel akademikPersonel)
        {
            using (var context = new DuzceUniversiteContext())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == akademikPersonel.UserId);

                        var userOperationClaims = await context.UserOperationClaims
                            .Where(uoc => uoc.UserId == user.Id)
                            .ToListAsync();

                        context.UserOperationClaims.RemoveRange(userOperationClaims);

                        context.Users.Remove(user);

                        context.AkademikPersonel.Remove(akademikPersonel);

                        await context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        private string GenerateUserName(AkademikPersonel personel)
        {
            var nameParts = personel.Ad.Split(' ');
            var initials = string.Join("", nameParts.Select(p => p[0]));
            return $"{personel.Id}{initials}";
        }
    }
}
