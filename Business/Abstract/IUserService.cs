using Core.Entities.Concrete;
using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IUserService
    {
        IResult Add(User user);
        IResult Delete(User user);
        IResult Update(User user);
        IDataResult<User> GetByMail(string email);
        IDataResult<User> GetById(int id);
        IDataResult<User> GetByUserName(string userName);
        IDataResult<List<OperationClaim>> GetClaims(User user);
        IDataResult<List<int>> GetBolumIds(int userId);
        IDataResult<List<User>> GetAll();
        IDataResult<PagedResult<User>> GetPagedList(PaginationParams paginationParams);
    }
}
