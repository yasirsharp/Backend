using Core.Utilities.Results;
using Entity.Concrete;
using Entity.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IBolumAkademikPersonellerService
    {
        IResult Add(BolumAkademikPersoneller bolumAkademikPersoneller);
        IResult Update(BolumAkademikPersoneller bolumAkademikPersoneller);
        IResult Delete(BolumAkademikPersoneller bolumAkademikPersoneller);

        IDataResult<BolumAkademikPersoneller> GetById(int id);
        IDataResult<List<BolumAkademikPersoneller>> GetAll();
        
        IDataResult<BolumAkademikPersonelListDTO> GetAkademikPersonellerByBolumId(int bolumId);
        IDataResult<AkademikPersonelBolumListDTO> GetBolumlerByAkademikPersonelId(int akademikPersonelId);
    
    }
}
