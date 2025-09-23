using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Concrete;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class DerslikManager : IDerslikService
    {
        private IDerslikDal _derslikDal;

        public DerslikManager(IDerslikDal derslikDal)
        {
            _derslikDal = derslikDal;
        }
        IResult IDerslikService.Add(Derslik derslik)
        {
            _derslikDal.Add(derslik);
            return new SuccessResult(Messages.DerslikAdded);
        }

        IResult IDerslikService.Delete(Derslik derslik)
        {
            _derslikDal.Delete(derslik);
            return new SuccessResult(Messages.DerslikDeleted);
        }

        IDataResult<Derslik> IDerslikService.GetById(int derslikId)
        {
            var result = _derslikDal.Get(q=>q.Id == derslikId);
            return new SuccessDataResult<Derslik>(result);
        }

        IDataResult<List<Derslik>> IDerslikService.GetList()
        {
            var result = _derslikDal.GetAll();
            return new SuccessDataResult<List<Derslik>>(result, $"{result.Count} tane sonuç bulundu.");
        }

        IResult IDerslikService.Update(Derslik derslik)
        {
            _derslikDal.Update(derslik);
            return new SuccessResult(Messages.DerslikUpdated);
        }
    }
}
