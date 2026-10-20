using Business.Abstract;
using Business.Constants;
using Business.Helpers;
using Business.ValidationRules.FluentValidation;
using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using DataAccess.Abstract;
using Entity.Concrete;
using Entity.DTOs;

namespace Business.Concrete
{
    /// <summary>
    /// Ogrenci Business Manager
    /// Öğrenci business logic operasyonlarını yönetir
    /// </summary>
    public class OgrenciManager : IOgrenciService
    {
        public const int MaxBulkRows = 1000;
        private const string DefaultEmailDomain = "ogr.duzce.edu.tr";

        private readonly IOgrenciDal _ogrenciDal;
        private readonly IUserDal _userDal;
        private readonly IBolumDal _bolumDal;

        public OgrenciManager(IOgrenciDal ogrenciDal, IUserDal userDal, IBolumDal bolumDal)
        {
            _ogrenciDal = ogrenciDal;
            _userDal = userDal;
            _bolumDal = bolumDal;
        }

        public async Task<IDataResult<CreatedAccountDto>> CreateAsync(OgrenciCreateDto dto, string? password = null)
        {
            var validation = new OgrenciCreateDtoValidator().Validate(dto);
            if (!validation.IsValid)
                return new ErrorDataResult<CreatedAccountDto>(validation.Errors[0].ErrorMessage);

            if (_bolumDal.Get(b => b.Id == dto.BolumId) == null)
                return new ErrorDataResult<CreatedAccountDto>("Seçilen bölüm bulunamadı.");

            return await CreateValidatedAsync(dto, password);
        }

        public async Task<IDataResult<BulkResultDto>> BulkCreateAsync(List<OgrenciCreateDto> rows)
        {
            if (rows == null || rows.Count == 0)
                return new ErrorDataResult<BulkResultDto>("Kaydedilecek satır yok.");
            if (rows.Count > MaxBulkRows)
                return new ErrorDataResult<BulkResultDto>($"Tek seferde en fazla {MaxBulkRows} satır aktarılabilir.");

            var bolumIds = _bolumDal.GetAll().Select(b => b.Id).ToHashSet();
            var validator = new OgrenciCreateDtoValidator();
            var result = new BulkResultDto();

            for (var i = 0; i < rows.Count; i++)
            {
                var row = new BulkRowResultDto { Row = i + 1 };
                var validation = validator.Validate(rows[i]);
                if (!validation.IsValid)
                    row.Message = validation.Errors[0].ErrorMessage;
                else if (!bolumIds.Contains(rows[i].BolumId))
                    row.Message = "Bölüm bulunamadı.";
                else
                {
                    var created = await CreateValidatedAsync(rows[i], null);
                    row.Success = created.Success;
                    row.Message = created.Message;
                    row.Account = created.Data;
                }
                result.Rows.Add(row);
            }

            result.SuccessCount = result.Rows.Count(r => r.Success);
            result.ErrorCount = result.Rows.Count - result.SuccessCount;
            return new SuccessDataResult<BulkResultDto>(result,
                $"{result.SuccessCount} öğrenci eklendi, {result.ErrorCount} satır hatalı.");
        }

        private async Task<IDataResult<CreatedAccountDto>> CreateValidatedAsync(OgrenciCreateDto dto, string? password)
        {
            var ogrenciNo = dto.OgrenciNo.Trim();
            var userName = ogrenciNo.ToLowerInvariant();
            var email = string.IsNullOrWhiteSpace(dto.Email)
                ? $"{userName}@{DefaultEmailDomain}"
                : dto.Email.Trim().ToLowerInvariant();

            if (_ogrenciDal.Get(o => o.OgrenciNo == ogrenciNo) != null)
                return new ErrorDataResult<CreatedAccountDto>($"Bu öğrenci numarası ({ogrenciNo}) zaten kayıtlı.");
            if (_userDal.Get(u => u.UserName == userName) != null)
                return new ErrorDataResult<CreatedAccountDto>($"'{userName}' kullanıcı adı zaten kullanılıyor.");
            if (_userDal.Get(u => u.Email == email) != null)
                return new ErrorDataResult<CreatedAccountDto>($"'{email}' e-posta adresi zaten kullanılıyor.");

            var user = AccountFactory.NewUser(dto.Ad, dto.Soyad, email, userName, out var temporaryPassword, password);
            var ogrenci = new Ogrenci
            {
                OgrenciNo = ogrenciNo,
                Ad = dto.Ad.Trim(),
                Soyad = dto.Soyad.Trim(),
                BolumId = dto.BolumId,
                Sinif = dto.Sinif,
                CreatedDate = DateTime.Now,
                Status = true
            };

            try
            {
                await _ogrenciDal.AddWithUserAsync(user, ogrenci, Roles.Ogrenci);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<CreatedAccountDto>(ExceptionLogger.Log(ex,
                    "Öğrenci kaydedilemedi (öğrenci no, kullanıcı adı veya e-posta başka bir kayıtta kullanılıyor olabilir)."));
            }

            return new SuccessDataResult<CreatedAccountDto>(
                AccountFactory.ToCreatedAccount(user, temporaryPassword), Messages.OgrenciAdded);
        }

        public async Task<IResult> UpdateAsync(OgrenciUpdateDto dto)
        {
            var validation = new OgrenciUpdateDtoValidator().Validate(dto);
            if (!validation.IsValid)
                return new ErrorResult(validation.Errors[0].ErrorMessage);

            var ogrenci = _ogrenciDal.Get(o => o.Id == dto.Id);
            if (ogrenci == null)
                return new ErrorResult(Messages.OgrenciNotFound);

            var ogrenciNo = dto.OgrenciNo.Trim();
            if (_ogrenciDal.Get(o => o.OgrenciNo == ogrenciNo && o.Id != dto.Id) != null)
                return new ErrorResult($"Bu öğrenci numarası ({ogrenciNo}) başka bir kayıt tarafından kullanılıyor.");
            if (_bolumDal.Get(b => b.Id == dto.BolumId) == null)
                return new ErrorResult("Seçilen bölüm bulunamadı.");

            // Sadece izin verilen alanlar değişir; UserId istemciden alınmaz
            ogrenci.OgrenciNo = ogrenciNo;
            ogrenci.Ad = dto.Ad.Trim();
            ogrenci.Soyad = dto.Soyad.Trim();
            ogrenci.BolumId = dto.BolumId;
            ogrenci.Sinif = dto.Sinif;
            ogrenci.Status = dto.Status;
            ogrenci.UpdatedDate = DateTime.Now;

            await _ogrenciDal.UpdateWithUserAsync(ogrenci);
            return new SuccessResult(Messages.OgrenciUpdated);
        }

        public async Task<IResult> DeleteAsync(int ogrenciId)
        {
            var ogrenci = _ogrenciDal.Get(o => o.Id == ogrenciId);
            if (ogrenci == null)
                return new ErrorResult(Messages.OgrenciNotFound);

            await _ogrenciDal.DeleteWithUserAsync(ogrenci);
            return new SuccessResult(Messages.OgrenciDeleted);
        }

        public IDataResult<Ogrenci> GetById(int ogrenciId)
        {
            var ogrenci = _ogrenciDal.Get(o => o.Id == ogrenciId);
            if (ogrenci == null)
            {
                return new ErrorDataResult<Ogrenci>($"ID: {ogrenciId} öğrenci bulunamadı.");
            }
            return new SuccessDataResult<Ogrenci>(ogrenci);
        }

        public IDataResult<Ogrenci> GetByUserId(int userId)
        {
            var ogrenci = _ogrenciDal.Get(o => o.UserId == userId);
            if (ogrenci == null)
            {
                return new ErrorDataResult<Ogrenci>($"UserId: {userId} için öğrenci kaydı bulunamadı.");
            }
            return new SuccessDataResult<Ogrenci>(ogrenci);
        }

        public IDataResult<Ogrenci> GetByOgrenciNo(string ogrenciNo)
        {
            var ogrenci = _ogrenciDal.Get(o => o.OgrenciNo == ogrenciNo);
            if (ogrenci == null)
            {
                return new ErrorDataResult<Ogrenci>($"Öğrenci No: {ogrenciNo} bulunamadı.");
            }
            return new SuccessDataResult<Ogrenci>(ogrenci);
        }

        public IDataResult<List<Ogrenci>> GetList()
        {
            var ogrenciler = _ogrenciDal.GetAll();
            return new SuccessDataResult<List<Ogrenci>>(
                ogrenciler,
                $"{ogrenciler.Count} öğrenci bulundu."
            );
        }

        public IDataResult<PagedResult<Ogrenci>> GetPagedList(PaginationParams paginationParams)
        {
            // Arama terimi varsa filtrele (Ad, Soyad, OgrenciNo)
            if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
            {
                var pagedResult = _ogrenciDal.GetPaged(
                    paginationParams,
                    o => o.Ad.Contains(paginationParams.SearchTerm) ||
                         o.Soyad.Contains(paginationParams.SearchTerm) ||
                         o.OgrenciNo.Contains(paginationParams.SearchTerm)
                );
                return new SuccessDataResult<PagedResult<Ogrenci>>(
                    pagedResult,
                    $"{pagedResult.TotalCount} öğrenci bulundu."
                );
            }

            // Arama terimi yoksa hepsini getir
            var result = _ogrenciDal.GetPaged(paginationParams);
            return new SuccessDataResult<PagedResult<Ogrenci>>(
                result,
                $"{result.TotalCount} öğrenci bulundu."
            );
        }
    }
}
