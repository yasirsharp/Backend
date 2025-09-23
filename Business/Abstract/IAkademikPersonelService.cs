using Core.Utilities.Results;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IAkademikPersonelService
    {
        IDataResult<List<AkademikPersonel>> GetList(Expression<Func<AkademikPersonel, bool>> filter = null);
        IDataResult<AkademikPersonel> GetById(int akademikPeronelId);
        IResult Add(AkademikPersonel akademikPersonel);
        IResult Delete(AkademikPersonel akademikPersonel);
        IResult Update(AkademikPersonel akademikPersonel);
    }
}
