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
    public class DersBolumManager : IDersBolumService
    {
        IDersBolumDal _dersBolumDal;

        public DersBolumManager(IDersBolumDal dersBolumDal)
        {
            _dersBolumDal = dersBolumDal;
        }

        public IResult Add(DersBolum dersBolum)
        {
            dersBolum.CreatedDate = DateTime.Now;
            _dersBolumDal.Add(dersBolum);
            return new SuccessResult(Messages.DersBolumAdded);
        }

        public IResult AddMultiple(List<DersBolum> dersBolumler)
        {
            foreach (var dersBolum in dersBolumler)
            {
                dersBolum.CreatedDate = DateTime.Now;
                _dersBolumDal.Add(dersBolum);
            }
            return new SuccessResult($"{dersBolumler.Count} adet " + Messages.DersBolumAdded);
        }

        public IResult Delete(DersBolum dersBolum)
        {
            _dersBolumDal.Delete(dersBolum);
            return new SuccessResult(Messages.DersBolumDeleted);
        }

        public IResult DeleteByDersId(int dersId)
        {
            var dersBolumler = _dersBolumDal.GetByDersId(dersId);
            foreach (var dersBolum in dersBolumler)
            {
                _dersBolumDal.Delete(dersBolum);
            }
            return new SuccessResult($"{dersBolumler.Count} adet " + Messages.DersBolumDeleted);
        }

        public IDataResult<DersBolum> GetById(int dersBolumId)
        {
            return new SuccessDataResult<DersBolum>(_dersBolumDal.Get(db => db.Id == dersBolumId));
        }

        public IDataResult<List<DersBolum>> GetByBolumId(int bolumId)
        {
            return new SuccessDataResult<List<DersBolum>>(_dersBolumDal.GetByBolumId(bolumId));
        }

        public IDataResult<List<DersBolum>> GetByDersId(int dersId)
        {
            return new SuccessDataResult<List<DersBolum>>(_dersBolumDal.GetByDersId(dersId));
        }

        public IDataResult<List<DersBolum>> GetList()
        {
            return new SuccessDataResult<List<DersBolum>>(_dersBolumDal.GetAll(), $"{_dersBolumDal.GetAll().Count} tane bulundu.");
        }

        public IResult Update(DersBolum dersBolum)
        {
            _dersBolumDal.Update(dersBolum);
            return new SuccessResult(Messages.DersBolumUpdated);
        }
    }
}
