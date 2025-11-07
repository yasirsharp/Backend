using Core.DataAccess;
using Entity.Concrete;

namespace DataAccess.Abstract
{
    /// <summary>
    /// Ogrenci Data Access Interface
    /// Öğrenci veritabanı işlemleri için repository interface
    /// </summary>
    public interface IOgrenciDal : IEntityRepository<Ogrenci>
    {
        // Standart CRUD işlemleri IEntityRepository'den geliyor
        // Gerekirse özel metodlar buraya eklenebilir (örn: GetByOgrenciNo)
    }
}
