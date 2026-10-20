using Business.Abstract;
using Business.Constants;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Validation;
using Core.Entities.Concrete;
using Core.Utilities.Results;
using Core.Utilities.Security.Hashing;
using Core.Utilities.Security.JWT;
using Entity.DTOs;
using Entity.Concrete;
using DataAccess.Abstract;
using System.Security.Claims;
using Business.Helpers;

namespace Business.Concrete
{
    public class AuthManager : IAuthService
    {
        // Kullanıcı bulunamadığında da parola doğrulaması yapılır; böylece yanıt süresinden
        // e-postanın kayıtlı olup olmadığı anlaşılamaz.
        private static readonly byte[] DummySalt = new byte[16];
        private static readonly byte[] DummyHash = new byte[64];

        private readonly IUserService _userService;
        private readonly ITokenHelper _tokenHelper;
        private readonly IOgrenciService _ogrenciService;
        private readonly IAkademikPersonelDal _akademikPersonelDal; // Transaction için direkt Dal kullanımı

        public AuthManager(IUserService userService,
            ITokenHelper tokenHelper,
            IOgrenciService ogrenciService,
            IAkademikPersonelDal akademikPersonelDal)
        {
            _userService = userService;
            _tokenHelper = tokenHelper;
            _ogrenciService = ogrenciService;
            _akademikPersonelDal = akademikPersonelDal;
        }

        /// <summary>
        /// Genel kayıt (Auth:AllowPublicRegistration açıksa). Kullanıcı parolasını kendisi seçtiği için
        /// parola değiştirme zorunluluğu yoktur; rol ataması yönetici tarafından yapılır.
        /// </summary>
        [ValidationAspect(typeof(UserForRegisterDtoValidator))]
        public IDataResult<User> Register(UserForRegisterDto userForRegisterDto, string password)
        {
            if (_userService.GetByUserName(userForRegisterDto.UserName).Data != null)
                return new ErrorDataResult<User>(Messages.UserNameAlreadyExists);

            var user = AccountFactory.NewUser(userForRegisterDto.FirstName, userForRegisterDto.LastName,
                userForRegisterDto.Email, userForRegisterDto.UserName, out _, password);
            user.MustChangePassword = false;

            var result = _userService.Add(user);
            if (!result.Success) return new ErrorDataResult<User>(result.Message);
            return new SuccessDataResult<User>(user, Messages.UserRegistered);
        }

        /// <summary>
        /// Yönetici tarafından tek öğrenci kaydı. Parola verilmezse geçici parola üretilir; her iki durumda da
        /// öğrenci ilk girişte parolasını değiştirir. Kullanıcı adı öğrenci numarasıdır.
        /// </summary>
        [ValidationAspect(typeof(OgrenciRegisterDtoValidator))]
        public Task<IDataResult<CreatedAccountDto>> RegisterOgrenciAsync(OgrenciRegisterDto dto)
        {
            return _ogrenciService.CreateAsync(new OgrenciCreateDto
            {
                OgrenciNo = dto.OgrenciNo,
                Ad = dto.FirstName,
                Soyad = dto.LastName,
                Email = dto.Email,
                BolumId = dto.BolumId,
                Sinif = dto.Sinif
            }, dto.Password);
        }

        [ValidationAspect(typeof(AkademikPersonelRegisterDtoValidator))]
        public async Task<IDataResult<User>> RegisterAkademikPersonelAsync(AkademikPersonelRegisterDto akademikPersonelRegisterDto)
        {
            var userName = AccountFactory.ToAsciiLower(akademikPersonelRegisterDto.Email.Split('@')[0]); // Email'in @ öncesi
            if (_userService.GetByUserName(userName).Data != null)
                return new ErrorDataResult<User>(Messages.UserNameAlreadyExists);

            // Yönetici parolayı belirlese de kullanıcı ilk girişte değiştirir
            var user = AccountFactory.NewUser(akademikPersonelRegisterDto.FirstName, akademikPersonelRegisterDto.LastName,
                akademikPersonelRegisterDto.Email, userName, out _, akademikPersonelRegisterDto.Password);

            var akademikPersonel = new AkademikPersonel
            {
                Ad = akademikPersonelRegisterDto.FirstName + " " + akademikPersonelRegisterDto.LastName,
                Unvan = "Unvan-" + akademikPersonelRegisterDto.UnvanId.ToString(), // TODO: Unvan tablosundan çekilecek
                CreatedDate = DateTime.Now,
                Status = true
                // UserId Dal tarafından atanacak
            };

            // Transaction içinde kaydet (User → AkademikPersonel → Rol). Hata olursa DAL transaction'ı geri alır
            // ve istisna ExceptionMiddleware'de loglanır.
            await _akademikPersonelDal.AddWithUserAsync(user, akademikPersonel, Roles.AkademikPersonel);

            return new SuccessDataResult<User>(user, "Akademik personel kaydı başarıyla oluşturuldu");
        }

        [ValidationAspect(typeof(ChangePasswordDtoValidator))]
        public IDataResult<User> ChangePassword(int userId, ChangePasswordDto dto)
        {
            var userResult = _userService.GetById(userId);
            var user = userResult.Success ? userResult.Data : null;
            if (user == null || !user.Status)
                return new ErrorDataResult<User>(Messages.UserNotFound);

            if (!HashingHelper.VerifyPasswordHash(dto.CurrentPassword, user.PasswordHash, user.PasswordSalt))
                return new ErrorDataResult<User>("Mevcut parola hatalı.");

            HashingHelper.CreatePasswordHash(dto.NewPassword, out var hash, out var salt);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.MustChangePassword = false;
            user.UpdatedDate = DateTime.Now;

            var update = _userService.Update(user);
            if (!update.Success)
                return new ErrorDataResult<User>(update.Message);

            return new SuccessDataResult<User>(user, "Parolanız değiştirildi.");
        }

        public IDataResult<CreatedAccountDto> ResetPassword(int targetUserId, bool actorIsSuperAdmin)
        {
            var userResult = _userService.GetById(targetUserId);
            var user = userResult.Success ? userResult.Data : null;
            if (user == null)
                return new ErrorDataResult<CreatedAccountDto>(Messages.UserNotFound);

            var targetRoles = _userService.GetClaims(user).Data ?? new List<OperationClaim>();
            if (!actorIsSuperAdmin && targetRoles.Any(r => r.Name == Roles.SuperAdmin))
                return new ErrorDataResult<CreatedAccountDto>("super.admin hesabının parolasını yalnızca super.admin sıfırlayabilir.");

            var temporaryPassword = PasswordGenerator.Generate();
            HashingHelper.CreatePasswordHash(temporaryPassword, out var hash, out var salt);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.MustChangePassword = true;
            user.UpdatedDate = DateTime.Now;

            var update = _userService.Update(user);
            if (!update.Success)
                return new ErrorDataResult<CreatedAccountDto>(update.Message);

            return new SuccessDataResult<CreatedAccountDto>(AccountFactory.ToCreatedAccount(user, temporaryPassword),
                "Parola sıfırlandı. Kullanıcı ilk girişte yeni parola belirleyecek.");
        }

        public IDataResult<User> Login(UserForLoginDto userForLoginDto)
        {
            if (string.IsNullOrWhiteSpace(userForLoginDto.Password) ||
                (string.IsNullOrWhiteSpace(userForLoginDto.Email) && string.IsNullOrWhiteSpace(userForLoginDto.UserName)))
            {
                return new ErrorDataResult<User>(Messages.InvalidCredentials);
            }

            var userToCheck = !string.IsNullOrWhiteSpace(userForLoginDto.Email)
                ? _userService.GetByMail(userForLoginDto.Email)
                : _userService.GetByUserName(userForLoginDto.UserName);

            var user = userToCheck.Success ? userToCheck.Data : null;

            // "Kullanıcı bulunamadı" ve "şifre hatalı" için aynı mesaj döner (kullanıcı numaralandırmasını önler)
            var passwordValid = HashingHelper.VerifyPasswordHash(
                userForLoginDto.Password,
                user?.PasswordHash ?? DummyHash,
                user?.PasswordSalt ?? DummySalt);

            if (user == null || !passwordValid)
            {
                return new ErrorDataResult<User>(Messages.InvalidCredentials);
            }

            if (!user.Status)
            {
                return new ErrorDataResult<User>(Messages.UserInactive);
            }

            // Eski (tek turluk HMACSHA512) hash'i yeni PBKDF2 formatına yükselt
            if (HashingHelper.NeedsRehash(user.PasswordSalt))
            {
                HashingHelper.CreatePasswordHash(userForLoginDto.Password, out var newHash, out var newSalt);
                var oldHash = user.PasswordHash;
                var oldSalt = user.PasswordSalt;
                user.PasswordHash = newHash;
                user.PasswordSalt = newSalt;
                try
                {
                    _userService.Update(user);
                }
                catch (Exception ex)
                {
                    // Yükseltme başarısız olsa da giriş engellenmez; bir sonraki girişte tekrar denenir
                    user.PasswordHash = oldHash;
                    user.PasswordSalt = oldSalt;
                    ExceptionLogger.Log(ex, "Parola hash'i yeni formata yükseltilemedi.");
                }
            }

            return new SuccessDataResult<User>(user, Messages.SuccessfulLogin);
        }

        public IResult UserExists(string email)
        {
            var result = _userService.GetByMail(email);
            if (result.Data != null)
            {
                return new ErrorResult(Messages.UserAlreadyExists);
            }
            return new SuccessResult();
        }

        public IDataResult<AccessToken> CreateAccessToken(User user)
        {
            var claims = _userService.GetClaims(user).Data;
            List<Claim> bolumIdleri = new List<Claim>();

            // Öğrenci role kontrolü
            if (claims != null && claims.Any(c => c.Name == Roles.Ogrenci))
            {
                var ogrenciResult = _ogrenciService.GetByUserId(user.Id);
                if (ogrenciResult.Success && ogrenciResult.Data != null)
                {
                    // Öğrenci için BolumId ve OgrenciId ekle
                    bolumIdleri.Add(new Claim("BolumId", ogrenciResult.Data.BolumId.ToString()));
                    bolumIdleri.Add(new Claim("OgrenciId", ogrenciResult.Data.Id.ToString()));
                    bolumIdleri.Add(new Claim("OgrenciNo", ogrenciResult.Data.OgrenciNo));
                }
            }
            // Bölüm görevlisi role kontrolü (mevcut logic)
            else if (claims != null && claims.Any(c => c.Name == Roles.BolumGorevlisi))
            {
                var userBolums = _userService.GetBolumIds(user.Id).Data;
                if (userBolums != null && userBolums.Count > 0)
                {
                    for (int i = 0; i < userBolums.Count; i++)
                    {
                        Claim claim = new Claim($"{i + 1}.BolumId", userBolums[i].ToString());
                        bolumIdleri.Add(claim);
                    }
                }
            }

            var accessToken = _tokenHelper.CreateToken(user, claims, bolumIdleri);
            return new SuccessDataResult<AccessToken>(accessToken, Messages.AccessTokenCreated);
        }
    }
}
