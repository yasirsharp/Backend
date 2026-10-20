using Core.DataAccess;
using Core.Entities.Concrete;
using Entity.Concrete;

namespace DataAccess.Abstract
{
    /// <summary>
    /// Ogrenci Data Access Interface
    /// Öğrenci veritabanı işlemleri için repository interface
    /// </summary>
    public interface IOgrenciDal : IEntityRepository<Ogrenci>
    {
        /// <summary>Kullanıcı, öğrenci kaydı ve rolü tek transaction içinde ekler.</summary>
        Task AddWithUserAsync(User user, Ogrenci ogrenci, string roleName);

        /// <summary>Öğrenci ve kullanıcısının ad/soyad bilgilerini birlikte günceller.</summary>
        Task UpdateWithUserAsync(Ogrenci ogrenci);

        /// <summary>Öğrenci kaydını siler; kullanıcı pasifleştirilir ve rolleri kaldırılır.</summary>
        Task DeleteWithUserAsync(Ogrenci ogrenci);
    }
}
