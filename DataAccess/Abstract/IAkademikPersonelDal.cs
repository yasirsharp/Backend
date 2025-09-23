using Core.DataAccess;
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
        void AddAkademikPersonelWithUserOperationClaim(AkademikPersonel akademikPersonel);
        void UpdateAkademikPersonelWithUserOperationClaim(AkademikPersonel akademikPersonel);
        void DeleteAkademikPersonelWithUserOperationClaim(AkademikPersonel akademikPersonel);
    }
}
