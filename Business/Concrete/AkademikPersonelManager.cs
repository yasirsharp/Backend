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
using Business.Helpers;
using Entity.DTOs;

namespace Business.Concrete
{
    public class AkademikPersonelManager : IAkademikPersonelService
    {
        private readonly IAkademikPersonelDal _akademikPersonelDal;
        private readonly IUserService _userService;
        private readonly IDBAPDal _dBAPDal;
        private readonly IUserDal _userDal;
        private readonly IBolumAkademikPersonellerDal _bolumAkademikPersonellerDal;

        public AkademikPersonelManager(
            IAkademikPersonelDal akademikPersonelDal,
            IUserService userService,
            IDBAPDal dBAPDal,
            IUserDal userDal,
            IBolumAkademikPersonellerDal bolumAkademikPersonellerDal)
        {
            _akademikPersonelDal = akademikPersonelDal;
            _userService = userService;
            _dBAPDal = dBAPDal;
            _userDal = userDal;
            _bolumAkademikPersonellerDal = bolumAkademikPersonellerDal;
        }

        public const int MaxBulkRows = 1000;

        public async Task<IDataResult<CreatedAccountDto>> CreateAsync(AkademikPersonelCreateDto dto)
        {
            var validation = new AkademikPersonelCreateDtoValidator().Validate(dto);
            if (!validation.IsValid)
                return new ErrorDataResult<CreatedAccountDto>(validation.Errors[0].ErrorMessage);

            var fullName = string.Join(" ", dto.Ad.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var (firstName, lastName) = AccountFactory.SplitFullName(fullName);

            var userName = AccountFactory.GenerateUserName(fullName, dto.Email,
                candidate => _userDal.Get(u => u.UserName == candidate) != null);
            var email = string.IsNullOrWhiteSpace(dto.Email)
                ? $"{userName}@duzce.edu.tr"
                : dto.Email.Trim().ToLowerInvariant();

            if (_userDal.Get(u => u.Email == email) != null)
                return new ErrorDataResult<CreatedAccountDto>($"'{email}' e-posta adresi zaten kullanılıyor.");

            // Parola kriptografik olarak rastgele üretilir, yöneticiye bir kez gösterilir ve ilk girişte değiştirilir.
            var user = AccountFactory.NewUser(firstName, lastName, email, userName, out var temporaryPassword);
            var akademikPersonel = new AkademikPersonel
            {
                Ad = fullName,
                Unvan = dto.Unvan.Trim(),
                CreatedDate = DateTime.Now,
                Status = true
            };

            try
            {
                await _akademikPersonelDal.AddWithUserAsync(user, akademikPersonel, Roles.AkademikPersonel);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<CreatedAccountDto>(ExceptionLogger.Log(ex,
                    "Akademik personel kaydedilemedi (kullanıcı adı veya e-posta başka bir kayıtta kullanılıyor olabilir)."));
            }

            // Mesaj biçimi frontend'in eski sürümüyle uyumlu tutulur; bilgiler Data içinde de döner.
            return new SuccessDataResult<CreatedAccountDto>(
                AccountFactory.ToCreatedAccount(user, temporaryPassword),
                $"{Messages.AkademikPersonelAdded} Kullanıcı adı: {userName}, geçici parola: {temporaryPassword}");
        }

        public async Task<IDataResult<BulkResultDto>> BulkCreateAsync(List<AkademikPersonelCreateDto> rows)
        {
            if (rows == null || rows.Count == 0)
                return new ErrorDataResult<BulkResultDto>("Kaydedilecek satır yok.");
            if (rows.Count > MaxBulkRows)
                return new ErrorDataResult<BulkResultDto>($"Tek seferde en fazla {MaxBulkRows} satır aktarılabilir.");

            var result = new BulkResultDto();
            for (var i = 0; i < rows.Count; i++)
            {
                var created = await CreateAsync(rows[i]);
                result.Rows.Add(new BulkRowResultDto
                {
                    Row = i + 1,
                    Success = created.Success,
                    // Toplu sonuçta parola mesajda tekrar edilmez, sadece Account içinde döner
                    Message = created.Success ? Messages.AkademikPersonelAdded : created.Message,
                    Account = created.Data
                });
            }

            result.SuccessCount = result.Rows.Count(r => r.Success);
            result.ErrorCount = result.Rows.Count - result.SuccessCount;
            return new SuccessDataResult<BulkResultDto>(result,
                $"{result.SuccessCount} personel eklendi, {result.ErrorCount} satır hatalı.");
        }

        public IDataResult<List<AkademikPersonel>> GetByBolumId(int bolumId)
        {
            var personelIds = _dBAPDal.GetAll(x => x.BolumId == bolumId).Select(x => x.AkademikPersonelId)
                .Concat(_bolumAkademikPersonellerDal.GetAll(x => x.BolumId == bolumId).Select(x => x.AkademikPersonelId))
                .Distinct()
                .ToList();

            var list = _akademikPersonelDal.GetAll(x => x.Status && personelIds.Contains(x.Id));
            return new SuccessDataResult<List<AkademikPersonel>>(list, $"{list.Count} akademik personel bulundu.");
        }

        public async Task<IResult> Update(AkademikPersonel akademikPersonel)
        {
            if (string.IsNullOrWhiteSpace(akademikPersonel.Ad) || string.IsNullOrWhiteSpace(akademikPersonel.Unvan))
                return new ErrorResult("Ad ve unvan boş olamaz.");

            // Kullanıcı bağlantısı istemciden alınmaz: aksi halde başka bir kullanıcının adı değiştirilebilirdi
            var existing = _akademikPersonelDal.Get(x => x.Id == akademikPersonel.Id);
            if (existing == null)
                return new ErrorResult(Messages.AkademikPersonelNotFound);
            akademikPersonel.UserId = existing.UserId;
            akademikPersonel.CreatedDate = existing.CreatedDate;
            akademikPersonel.Status = existing.Status;

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
            // Silinen (pasifleştirilen) personel listelerde görünmez
            var list = _akademikPersonelDal.GetAll(filter ?? (x => x.Status));
            return new SuccessDataResult<List<AkademikPersonel>>(list);
        }

        public IDataResult<PagedResult<AkademikPersonel>> GetPagedList(PaginationParams paginationParams)
        {
            var search = paginationParams.SearchTerm?.ToLower();
            Expression<Func<AkademikPersonel, bool>> filter = x => x.Status;

            if (!string.IsNullOrWhiteSpace(search))
                filter = x => x.Status && x.Ad.ToLower().Contains(search);

            var result = _akademikPersonelDal.GetPaged(paginationParams, filter);
            return new SuccessDataResult<PagedResult<AkademikPersonel>>(result);
        }
    }
}
