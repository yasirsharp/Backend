using Business.Abstract;
using Business.Constants;
using Core.Entities.Concrete;
using Core.Utilities.Results;
using DataAccess.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class UserManager : IUserService
    {
        private IUserDal _userDal;

        public UserManager(IUserDal userDal)
        {
            _userDal = userDal;
        }

        public IResult Add(User user)
        {
            try
            {
                _userDal.Add(user);
                return new SuccessResult(Messages.UserAdded);
            }
            catch (Exception err)
            {
                return new ErrorResult("Kullanıcı eklenemedi. " + Messages.SomethingWrong + "\n" + err.Message);
            }
        }

        public IResult Delete(User user)
        {
            try
            {
                var deletedUser = _userDal.Get(q=>q.Id == user.Id);
                if (deletedUser==null)
                {
                    return new ErrorResult(Messages.UserNotFound);
                }
                _userDal.Delete(user);
                return new SuccessResult(Messages.UserDeleted);
            }
            catch (Exception err)
            {
                return new ErrorResult(err.Message);
            }
        }

        public IDataResult<List<User>> GetAll()
        {
            try
            {
                return new SuccessDataResult<List<User>>(_userDal.GetAll(), $"{_userDal.GetAll().Count} tane sonuç bulundu.");
            }
            catch (Exception err)
            {
                return new ErrorDataResult<List<User>>(Messages.SomethingWrong + " " + err.Message);
            }
        }

        public IDataResult<List<int>> GetBolumIds(int userId)
        {
            try
            {
                var result = _userDal.GetBolumIds(userId);
                return new SuccessDataResult<List<int>>(result);
            }
            catch (Exception err)
            {
                return new ErrorDataResult<List<int>>(Messages.SomethingWrong + " " + err.Message);
            }
        }

        public IDataResult<User> GetById(int id)
        {
            try
            {
                var result = _userDal.Get(q=>q.Id == id);
                if (result == null) return new ErrorDataResult<User>("Kullanıcı bulunamadı");
                return new SuccessDataResult<User>(result);
            }
            catch (Exception err)
            {
                return new ErrorDataResult<User>(Messages.SomethingWrong + " " + err.Message);
            }
        }

        public IDataResult<User> GetByMail(string email)
        {
            try
            {
                var result = _userDal.Get(q=>q.Email == email);
                if (result == null) return new ErrorDataResult<User>(Messages.UserNotFound);

                return new SuccessDataResult<User>(result);
            }
            catch (Exception err)
            {
                return new ErrorDataResult<User>(Messages.SomethingWrong + " " + err.Message);
            }
        }

        public IDataResult<User> GetByUserName(string userName)
        {
            try
            {

                var result = _userDal.Get(q => q.UserName == userName);
                if (result == null) return new ErrorDataResult<User>(Messages.UserNotFound);

                return new SuccessDataResult<User>(result);
            }
            catch (Exception err)
            {
                return new ErrorDataResult<User>(Messages.SomethingWrong + " " + err.Message);
            }
        }

        public IDataResult<List<OperationClaim>> GetClaims(User user)
        {
            try
            {
                var result = _userDal.GetClaims(user);
                if (result.Count<=0) return new ErrorDataResult<List<OperationClaim>>(Messages.UserClaimsNotFound);

                return new SuccessDataResult<List<OperationClaim>>(result);
            }
            catch (Exception err)
            {
                return new ErrorDataResult<List<OperationClaim>>(Messages.SomethingWrong);
            }
        }

        public IResult Update(User user)
        {
            _userDal.Update(user);
            return new SuccessResult(Messages.UserUpdated);
        }
    }
}
