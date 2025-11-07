using Core.DataAccess;
using Core.Entities.Concrete;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface IAkademikPersonelDal : IEntityRepository<AkademikPersonel>
    {
        /// <summary>
        /// YENİ - Doğru mimari: Business layer'dan HAZIR nesneler alır, sadece transaction yapar
        /// </summary>
        Task AddAkademikPersonelWithUserOperationClaimAsync(
            User user, 
            AkademikPersonel akademikPersonel, 
            UserOperationClaim userOperationClaim);

        /// <summary>
        /// YENİ - Doğru mimari: Business layer'dan HAZIR nesneler alır, sadece transaction yapar
        /// </summary>
        Task UpdateAkademikPersonelWithUserOperationClaimAsync(
            User user, 
            AkademikPersonel akademikPersonel);

        /// <summary>
        /// YENİ - Doğru mimari: Sadece transaction yönetimi
        /// </summary>
        Task DeleteAkademikPersonelWithUserOperationClaimAsync(AkademikPersonel akademikPersonel);

        /// <summary>
        /// ESKİ - AdminPanel için (geriye dönük uyumluluk)
        /// </summary>
        [Obsolete("İş kuralları içeriyor. AddAkademikPersonelWithUserOperationClaimAsync kullanın.")]
        void AddAkademikPersonelWithUserOperationClaim(AkademikPersonel akademikPersonel);
        
        [Obsolete("İş kuralları içeriyor. UpdateAkademikPersonelWithUserOperationClaimAsync kullanın.")]
        void UpdateAkademikPersonelWithUserOperationClaim(AkademikPersonel akademikPersonel);
        
        [Obsolete("DeleteAkademikPersonelWithUserOperationClaimAsync kullanın.")]
        void DeleteAkademikPersonelWithUserOperationClaim(AkademikPersonel akademikPersonel);
    }
}
