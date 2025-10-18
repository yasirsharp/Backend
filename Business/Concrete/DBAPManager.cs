using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entity.Concrete;
using Entity.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class DBAPManager : IDBAPService
    {
        IDBAPDal _dbapDal;
        ISinavDetayService _sinavDetayService;

        public DBAPManager(IDBAPDal dbapDal, ISinavDetayService sinavDetayService)
        {
            _dbapDal = dbapDal;
            _sinavDetayService = sinavDetayService;
        }

        public IResult Add(DersBolumAkademikPersonel dersBolumAkademikPersonel)
        {
            _dbapDal.Add(dersBolumAkademikPersonel);
            return new SuccessResult(Messages.DBAPAdded);
        }

        public IResult Delete(DersBolumAkademikPersonel dersBolumAkademikPersonel)
        {
            // Bu DBAP'ye bağlı sınav var mı kontrol et
            var response = _sinavDetayService.GetByDBAPId(dersBolumAkademikPersonel.Id);
            if (response.Success && response.Data != null && response.Data.Count > 0)
            {
                return new ErrorResult("Bu eşleştirmeye bağlı sınav kayıtları bulunduğu için silinemez.");
            }
            
            _dbapDal.Delete(dersBolumAkademikPersonel);
            return new SuccessResult(Messages.DBAPDeleted);
        }

        public IDataResult<DersBolumAkademikPersonel> GetById(int dbapId)
        {
            var result = _dbapDal.Get(q=>q.Id==dbapId);
            if (result == null)
                return new ErrorDataResult<DersBolumAkademikPersonel>(Messages.SomethingWrong);

            return new SuccessDataResult<DersBolumAkademikPersonel>(result);

        }

        public IDataResult<List<DersBolumAkademikPersonel>> GetAll()
        {
            var result = _dbapDal.GetAll().ToList();
            if (result != null) return new SuccessDataResult<List<DersBolumAkademikPersonel>>(result, $"{result.Count} tane bulundu.");

            return new ErrorDataResult<List<DersBolumAkademikPersonel>>(result, Messages.SomethingWrong);
        }

        public IResult Update(DersBolumAkademikPersonel dersBolumAkademikPersonel)
        {
            _dbapDal.Update(dersBolumAkademikPersonel);
            return new SuccessResult(Messages.DBAPUpdated);
        }

        public IDataResult<List<DersBolumAkademikPersonelDTO>> GetAllDetails()
        {
            var result = _dbapDal.GetDetails().ToList();
            return new SuccessDataResult<List<DersBolumAkademikPersonelDTO>>(result, $"{result.Count} tane bulundu.");
        }

        public IDataResult<DersBolumAkademikPersonelDTO> GetDetail(int dbapId)
        {
            try
            {
                var result = _dbapDal.GetDetail(dbapId);
                return new SuccessDataResult<DersBolumAkademikPersonelDTO>(result);
            }
            catch (Exception err)
            {
                return new ErrorDataResult<DersBolumAkademikPersonelDTO>(err.Message);
            }
        }

        public IDataResult<List<DersBolumAkademikPersonel>> GetByBolumId(int bolumId)
        {
            try
            {
                var result = _dbapDal.GetAll(q=>q.BolumId == bolumId);

                if (result == null) return new ErrorDataResult<List<DersBolumAkademikPersonel>>(result, "Veri bulunamadı sınav eklemeyi deneyin");

                return new SuccessDataResult<List<DersBolumAkademikPersonel>>(result);
            }
            catch (Exception err)
            {
                return new ErrorDataResult<List<DersBolumAkademikPersonel>>(Messages.SomethingWrong + " " + err.Message);
            }
        }

        public IDataResult<List<DersBolumAkademikPersonelDTO>> GetDetailsByBolumId(int bolumId)
        {
            try
            {
                var result = _dbapDal.GetDetails(q=>q.BolumId == bolumId);
                return result == null
                    ? new SuccessDataResult<List<DersBolumAkademikPersonelDTO>>(result, "Veri bulunamadı sınav eklemeyi deneyin")
                    : new SuccessDataResult<List<DersBolumAkademikPersonelDTO>>(result);
            }
            catch (Exception err)
            {
                return new ErrorDataResult<List<DersBolumAkademikPersonelDTO>>(Messages.SomethingWrong + " " + err.Message);
            }
        }
    }
}
