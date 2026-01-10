using Business.Abstract;
using Business.Constants;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Validation;
using Core.Utilities.Results;
using Core.Utilities.Security.Hashing;
using DataAccess.Abstract;
using Entity.Concrete;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Core.Utilites.Results.Pagination;

namespace Business.Concrete
{
    public class AkademikPersonelManager : IAkademikPersonelService
    {
        private readonly IAkademikPersonelDal _akademikPersonelDal;
        private readonly IUserService _userService;
        private readonly IDBAPDal _dBAPDal;

        public AkademikPersonelManager(
            IAkademikPersonelDal akademikPersonelDal,
            IUserService userService,
            IDBAPDal dBAPDal)
        {
            _akademikPersonelDal = akademikPersonelDal;
            _userService = userService;
            _dBAPDal = dBAPDal;
        }

        [ValidationAspect(typeof(AkademikPersonelValidator))]
        public async Task<IResult> Add(AkademikPersonel akademikPersonel)
        {
            var parts = akademikPersonel.Ad.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var firstName = parts.Length > 1 ? string.Join(" ", parts.Take(parts.Length - 1)) : parts[0];
            var lastName = parts.Length > 1 ? parts.Last() : parts[0];

            var initials = string.Join("", parts.Select(p => p[0]));

            var number = Random.Shared.Next(1000, 10000);

            var userName = $"{initials}{number}";


            HashingHelper.CreatePasswordHash(userName, out var hash, out var salt);

            var user = new Core.Entities.Concrete.User
            {
                FirstName = firstName,
                LastName = lastName,
                UserName = userName,
                Email = $"{userName}@duzce.edu.tr",
                PasswordHash = hash,
                PasswordSalt = salt,
                Status = true,
                CreatedDate = DateTime.Now
            };

            var claim = new Core.Entities.Concrete.UserOperationClaim
            {
                OperationClaimId = 2,
                Status = true,
                CreatedDate = DateTime.Now
            };

            await _akademikPersonelDal.AddWithUserAsync(user, akademikPersonel, claim);
            return new SuccessResult(Messages.AkademikPersonelAdded);
        }

        public async Task<IResult> Update(AkademikPersonel akademikPersonel)
        {
            var userResult = _userService.GetById(akademikPersonel.UserId);
            if (!userResult.Success)
                return new ErrorResult(userResult.Message);

            var parts = akademikPersonel.Ad.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            userResult.Data.FirstName = parts.Length > 1
                ? string.Join(" ", parts.Take(parts.Length - 1))
                : parts[0];
            userResult.Data.LastName = parts.Length > 1 ? parts.Last() : parts[0];
            userResult.Data.UpdatedDate = DateTime.Now;

            akademikPersonel.UpdatedDate = DateTime.Now;

            await _akademikPersonelDal.UpdateWithUserAsync(userResult.Data, akademikPersonel);
            return new SuccessResult(Messages.AkademikPersonelUpdated);
        }

        public async Task<IResult> Delete(AkademikPersonel akademikPersonel)
        {
            var relationCount = _dBAPDal
                .GetDetails(x => x.AkademikPersonelId == akademikPersonel.Id)
                .Count;

            if (relationCount > 0)
                return new ErrorResult(Messages.AkademikPersonelHasRelations);

            await _akademikPersonelDal.DeleteWithUserAsync(akademikPersonel);
            return new SuccessResult(Messages.AkademikPersonelDeleted);
        }

        public IDataResult<AkademikPersonel> GetById(int akademikPersonelId)
        {
            return new SuccessDataResult<AkademikPersonel>(
                _akademikPersonelDal.Get(x => x.Id == akademikPersonelId));
        }

        public IDataResult<AkademikPersonel> GetByUserId(int userId)
        {
            var personel = _akademikPersonelDal.Get(x => x.UserId == userId);
            if (personel == null)
                return new ErrorDataResult<AkademikPersonel>(Messages.AkademikPersonelNotFoundForUser);

            return new SuccessDataResult<AkademikPersonel>(personel);
        }

        public IDataResult<List<AkademikPersonel>> GetList(Expression<Func<AkademikPersonel, bool>> filter = null)
        {
            var list = _akademikPersonelDal.GetAll(filter);
            return new SuccessDataResult<List<AkademikPersonel>>(list);
        }

        public IDataResult<PagedResult<AkademikPersonel>> GetPagedList(PaginationParams paginationParams)
        {
            var search = paginationParams.SearchTerm?.ToLower();
            Expression<Func<AkademikPersonel, bool>> filter = null;

            if (!string.IsNullOrWhiteSpace(search))
                filter = x => x.Ad.ToLower().Contains(search);

            var result = _akademikPersonelDal.GetPaged(paginationParams, filter);
            return new SuccessDataResult<PagedResult<AkademikPersonel>>(result);
        }
    }
}
