using Core.DataAccess;
using Core.Entities.Concrete;
using Entity.Concrete;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface IAkademikPersonelDal : IEntityRepository<AkademikPersonel>
    {
        /// <summary>Kullanıcı, personel kaydı ve rolü tek transaction içinde ekler.</summary>
        Task AddWithUserAsync(
            User user,
            AkademikPersonel akademikPersonel,
            string roleName);

        Task UpdateWithUserAsync(
            User user,
            AkademikPersonel akademikPersonel);

        Task DeleteWithUserAsync(AkademikPersonel akademikPersonel);
    }
}
