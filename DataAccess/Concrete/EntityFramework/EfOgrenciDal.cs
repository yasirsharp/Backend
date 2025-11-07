using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entity.Concrete;

namespace DataAccess.Concrete.EntityFramework
{
    /// <summary>
    /// Ogrenci Entity Framework Data Access Layer
    /// Öğrenci veritabanı işlemleri için EF implementasyonu
    /// </summary>
    public class EfOgrenciDal : EfEntityRepositoryBase<Ogrenci, DuzceUniversiteContext>, IOgrenciDal
    {
        // Standart CRUD işlemleri base class'tan geliyor
        // Gerekirse özel metodlar buraya eklenebilir
    }
}
