using Business.Abstract;
using Business.Constants;
using Core.Entities.Concrete;
using Entity.DTOs;
using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using DataAccess.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                return new ErrorResult(ExceptionLogger.Log(err, "Kullanıcı eklenemedi."));
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
                return new ErrorResult(ExceptionLogger.Log(err, Messages.SomethingWrong));
            }
        }

        public IDataResult<List<User>> GetAll()
        {
            try
            {
                var users = _userDal.GetAll();
                return new SuccessDataResult<List<User>>(users, $"{users.Count} tane sonuç bulundu.");
            }
            catch (Exception err)
            {
                return new ErrorDataResult<List<User>>(ExceptionLogger.Log(err, Messages.SomethingWrong));
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
                return new ErrorDataResult<List<int>>(ExceptionLogger.Log(err, Messages.SomethingWrong));
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
                return new ErrorDataResult<User>(ExceptionLogger.Log(err, Messages.SomethingWrong));
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
                return new ErrorDataResult<User>(ExceptionLogger.Log(err, Messages.SomethingWrong));
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
                return new ErrorDataResult<User>(ExceptionLogger.Log(err, Messages.SomethingWrong));
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

        public IDataResult<List<UserWithRolesDto>> GetAllWithRoles()
        {
            return new SuccessDataResult<List<UserWithRolesDto>>(_userDal.GetAllWithRoles());
        }

        public IResult Update(User user)
        {
            _userDal.Update(user);
            return new SuccessResult(Messages.UserUpdated);
        }

        public IDataResult<PagedResult<User>> GetPagedList(PaginationParams paginationParams)
        {
            try
            {
                var searchTerm = paginationParams.SearchTerm?.ToLower();
                Expression<Func<User, bool>> filter = null;

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    filter = u => u.FirstName.ToLower().Contains(searchTerm) ||
                                  u.LastName.ToLower().Contains(searchTerm) ||
                                  u.Email.ToLower().Contains(searchTerm) ||
                                  u.UserName.ToLower().Contains(searchTerm);
                }

                var pagedResult = _userDal.GetPaged(paginationParams, filter);
                return new SuccessDataResult<PagedResult<User>>(pagedResult, $"Toplam {pagedResult.TotalCount} kullanıcı bulundu.");
            }
            catch (Exception err)
            {
                return new ErrorDataResult<PagedResult<User>>(ExceptionLogger.Log(err, Messages.SomethingWrong));
            }
        }
    }
}
