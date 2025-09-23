using Business.Abstract;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entity.Concrete;
using Entity.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class BolumAkademikPersonelManager : IBolumAkademikPersonellerService
    {
        IBolumAkademikPersonellerDal _bolumAkademikPersonellerDal;

        public BolumAkademikPersonelManager(IBolumAkademikPersonellerDal bolumAkademikPersonellerDal)
        {
            _bolumAkademikPersonellerDal = bolumAkademikPersonellerDal;
        }

        public IResult Add(BolumAkademikPersoneller bolumAkademikPersoneller)
        {
            _bolumAkademikPersonellerDal.Add(bolumAkademikPersoneller);
            return new SuccessResult();
        }

        public IResult Delete(BolumAkademikPersoneller bolumAkademikPersoneller)
        {
            _bolumAkademikPersonellerDal.Delete(bolumAkademikPersoneller);
            return new SuccessResult();
        }

        public IDataResult<BolumAkademikPersoneller> GetById(int id)
        {
            var result = _bolumAkademikPersonellerDal.Get(q=>q.Id == id);
            return new SuccessDataResult<BolumAkademikPersoneller>(result);
        }

        public IDataResult<List<BolumAkademikPersoneller>> GetAll()
        {
            var result = _bolumAkademikPersonellerDal.GetAll();
            return new SuccessDataResult<List<BolumAkademikPersoneller>>(result);
        }

        public IResult Update(BolumAkademikPersoneller bolumAkademikPersoneller)
        {
            _bolumAkademikPersonellerDal.Update(bolumAkademikPersoneller);
            return new SuccessResult();
        }

        public IDataResult<BolumAkademikPersonelListDTO> GetAkademikPersonellerByBolumId(int bolumId)
        {
            var result = _bolumAkademikPersonellerDal.GetAkademikPersonellerByBolumId(bolumId);
            if (result == null)
                return new ErrorDataResult<BolumAkademikPersonelListDTO>("Bölüm bulunamadı.");

            return new SuccessDataResult<BolumAkademikPersonelListDTO>(result, "Bölüme atanmış akademik personeller başarıyla getirildi.");
        }

        public IDataResult<AkademikPersonelBolumListDTO> GetBolumlerByAkademikPersonelId(int akademikPersonelId)
        {
            var result = _bolumAkademikPersonellerDal.GetBolumlerByAkademikPersonelId(akademikPersonelId);
            if (result == null)
                return new ErrorDataResult<AkademikPersonelBolumListDTO>("Akademik personel bulunamadı.");

            return new SuccessDataResult<AkademikPersonelBolumListDTO>(result, "Akademik personelin atandığı bölümler başarıyla getirildi.");
        }
    }
}
