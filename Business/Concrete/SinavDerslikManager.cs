using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class SinavDerslikManager : ISinavDerslikService
    {
        private readonly ISinavDerslikDal _sinavDerslikDal;

        public SinavDerslikManager(ISinavDerslikDal sinavDerslikDal)
        {
            _sinavDerslikDal = sinavDerslikDal;
        }

        public IResult Add(SinavDerslik sinavDerslik)
        {
            try
            {
                _sinavDerslikDal.Add(sinavDerslik);

                return new SuccessResult(Messages.SinavDerslikAdded);
            }
            catch (Exception err)
            {
                return new ErrorResult(err.Message);
            }
        }

        public IResult Delete(SinavDerslik sinavDerslik)
        {
            try
            {
                _sinavDerslikDal.Delete(sinavDerslik);

                return new SuccessResult(Messages.SinavDerslikDeleted);
            }
            catch (Exception err)
            {
                return new ErrorResult(err.Message);
            }
        }

        public IDataResult<SinavDerslik> Get(int id)
        {
            try
            {
                var result = _sinavDerslikDal.Get(q=>q.Id==id);
                return new SuccessDataResult<SinavDerslik>(result);
            }
            catch (Exception err)
            {
                return new ErrorDataResult<SinavDerslik>(err.Message);
            }
        }

        public IDataResult<List<SinavDerslik>> GetAll()
        {
            try
            {
                var result = _sinavDerslikDal.GetAll();
                return new SuccessDataResult<List<SinavDerslik>>(result, $"{result.Count} sonuç bulundu.");
            }
            catch (Exception err)
            {
                return new ErrorDataResult<List<SinavDerslik>>(err.Message);
            }
        }

        public IDataResult<List<SinavDetay>> GetByDerslikId(int derslikId)
        {
            try
            {
                var result = _sinavDerslikDal.GetByDerslikId(derslikId);
                return new SuccessDataResult<List<SinavDetay>>(result, $"{result.Count} sonuç bulundu.");
            }
            catch (Exception err)
            {
                return new ErrorDataResult<List<SinavDetay>>(err.Message);
            }
        }

        public IDataResult<List<SinavDetay>> GetByGozetmenId(int gozetmenId)
        {
            try
            {
                var result = _sinavDerslikDal.GetByGozetmenId(gozetmenId);
                return new SuccessDataResult<List<SinavDetay>>(result, $"{result.Count} sonuç bulundu.");
            }
            catch (Exception err)
            {
                return new ErrorDataResult<List<SinavDetay>>(err.Message);
            }
        }

        public IDataResult<List<SinavDerslik>> GetBySinavDetayId(int sinavDetayId)
        {
            try
            {
                var result = _sinavDerslikDal.GetBySinavDetayId(sinavDetayId);
                return new SuccessDataResult<List<SinavDerslik>>(result);
            }
            catch (Exception err)
            {
                return new ErrorDataResult<List<SinavDerslik>>(err.Message);
            }
        }

        public IResult Update(SinavDerslik sinavDerslik)
        {
            try
            {
                _sinavDerslikDal.Update(sinavDerslik);

                return new SuccessResult(Messages.SinavDerslikUpdated);
            }
            catch (Exception err)
            {
                return new ErrorResult(err.Message);
            }
        }

    }
}
