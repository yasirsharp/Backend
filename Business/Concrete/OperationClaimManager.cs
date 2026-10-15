using Business.Abstract;
using Business.Constants;
using Core.Entities.Concrete;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entity.Concrete;

namespace Business.Concrete
{
    public class OperationClaimManager : IOperationClaimService
    {
        private readonly IOperationClaimDal _operationClaimDal;

        public OperationClaimManager(IOperationClaimDal operationClaimDal)
        {
            _operationClaimDal = operationClaimDal;
        }

        public IResult Add(OperationClaim operationClaim)
        {
            try
            {
                _operationClaimDal.Add(operationClaim);
                return new SuccessResult(Messages.OperationClaimAdded);
            }
            catch (Exception ex)
            {
                return new ErrorResult(ExceptionLogger.Log(ex, "Yetki eklenirken bir hata oluştu."));
            }
        }

        public IResult Delete(OperationClaim operationClaim)
        {
            try
            {
                _operationClaimDal.Delete(operationClaim);
                return new SuccessResult(Messages.OperationClaimDeleted);
            }
            catch (Exception ex)
            {
                return new ErrorResult(ExceptionLogger.Log(ex, "Yetki silinirken bir hata oluştu."));
            }
        }

        public IDataResult<List<OperationClaim>> GetAll()
        {
            try
            {
                var claims = _operationClaimDal.GetAll();
                return new SuccessDataResult<List<OperationClaim>>(claims, Messages.OperationClaimListed);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<OperationClaim>>(ExceptionLogger.Log(ex, "Yetkiler listelenirken bir hata oluştu."));
            }
        }

        public IDataResult<OperationClaim> GetById(int id)
        {
            try
            {
                var claim = _operationClaimDal.Get(c => c.Id == id);
                if (claim == null)
                {
                    return new ErrorDataResult<OperationClaim>("Yetki bulunamadı.");
                }
                return new SuccessDataResult<OperationClaim>(claim, Messages.OperationClaimListed);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<OperationClaim>(ExceptionLogger.Log(ex, "Yetki getirilirken bir hata oluştu."));
            }
        }

        public IResult Update(OperationClaim operationClaim)
        {
            try
            {
                _operationClaimDal.Update(operationClaim);
                return new SuccessResult(Messages.OperationClaimUpdated);
            }
            catch (Exception ex)
            {
                return new ErrorResult(ExceptionLogger.Log(ex, "Yetki güncellenirken bir hata oluştu."));
            }
        }
    }
} 