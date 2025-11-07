using Business.Abstract;
using Business.Constants;
using Core.Entities.Concrete;
using Core.Utilities.Results;
using Core.Utilities.Security.Hashing;
using Core.Utilities.Security.JWT;
using Entity.DTOs;
using Entity.Concrete;
using DataAccess.Abstract;
using System.Security.Claims;

namespace Business.Concrete
{
    public class AuthManager : IAuthService
    {
        private IUserService _userService;
        private ITokenHelper _tokenHelper;
        private IOgrenciService _ogrenciService;
        private IUserOperationClaimService _userOperationClaimService;
        private IAkademikPersonelDal _akademikPersonelDal; // Transaction için direkt Dal kullanımı

        public AuthManager(IUserService userService,
            ITokenHelper tokenHelper,
            IOgrenciService ogrenciService,
            IUserOperationClaimService userOperationClaimService,
            IAkademikPersonelDal akademikPersonelDal)
        {
            _userService = userService;
            _tokenHelper = tokenHelper;
            _ogrenciService = ogrenciService;
            _userOperationClaimService = userOperationClaimService;
            _akademikPersonelDal = akademikPersonelDal;
        }



        public IDataResult<User> Register(UserForRegisterDto userForRegisterDto, string password)
        {
            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash(password, out passwordHash, out passwordSalt);
            var user = new User
            {
                Email = userForRegisterDto.Email,
                UserName = userForRegisterDto.UserName,
                FirstName = userForRegisterDto.FirstName,
                LastName = userForRegisterDto.LastName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Status = true
            };
            var result =_userService.Add(user);
            if(!result.Success) return new ErrorDataResult<User>(result.Message);
            return new SuccessDataResult<User>(user, Messages.UserRegistered);
        }

        public IDataResult<User> RegisterOgrenci(OgrenciRegisterDto ogrenciRegisterDto)
        {
            // 1. Kullanıcı oluştur
            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash(ogrenciRegisterDto.Password, out passwordHash, out passwordSalt);
            var user = new User
            {
                Email = ogrenciRegisterDto.Email,
                UserName = ogrenciRegisterDto.Email.Split('@')[0], // Email'in @ öncesi
                FirstName = ogrenciRegisterDto.FirstName,
                LastName = ogrenciRegisterDto.LastName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Status = true
            };
            
            var userResult = _userService.Add(user);
            if (!userResult.Success) return new ErrorDataResult<User>(userResult.Message);
            
            // 2. Öğrenci kaydı oluştur
            var ogrenci = new Ogrenci
            {
                UserId = user.Id,
                OgrenciNo = ogrenciRegisterDto.OgrenciNo,
                Ad = ogrenciRegisterDto.FirstName,
                Soyad = ogrenciRegisterDto.LastName,
                BolumId = ogrenciRegisterDto.BolumId,
                Sinif = ogrenciRegisterDto.Sinif,
                CreatedDate = DateTime.Now,
                Status = true
            };
            
            var ogrenciResult = _ogrenciService.Add(ogrenci);
            if (!ogrenciResult.Success) 
            {
                // Rollback: User'ı sil
                _userService.Delete(user);
                return new ErrorDataResult<User>(ogrenciResult.Message);
            }
            
            // 3. Öğrenci role'ü ata (OperationClaimId = 5 varsayımı)
            var userOperationClaim = new Core.Entities.Concrete.UserOperationClaim
            {
                UserId = user.Id,
                OperationClaimId = 3, // "ogrenci" role'ü
                CreatedDate = DateTime.Now,
                Status = true
            };
            
            var claimResult = _userOperationClaimService.Add(userOperationClaim);
            if (!claimResult.Success)
            {
                // Rollback: User ve Öğrenci'yi sil
                _ogrenciService.Delete(ogrenci);
                _userService.Delete(user);
                return new ErrorDataResult<User>(claimResult.Message);
            }
            
            return new SuccessDataResult<User>(user, "Öğrenci kaydı başarıyla oluşturuldu");
        }

        public IDataResult<User> RegisterAkademikPersonel(AkademikPersonelRegisterDto akademikPersonelRegisterDto)
        {
            // 1. İŞ KURALI: Kullanıcı hazırla (password hashing - Business logic!)
            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash(akademikPersonelRegisterDto.Password, out passwordHash, out passwordSalt);
            var user = new User
            {
                Email = akademikPersonelRegisterDto.Email,
                UserName = akademikPersonelRegisterDto.Email.Split('@')[0], // Email'in @ öncesi
                FirstName = akademikPersonelRegisterDto.FirstName,
                LastName = akademikPersonelRegisterDto.LastName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Status = true,
                CreatedDate = DateTime.Now
            };
            
            // 2. İŞ KURALI: Akademik Personel hazırla
            var akademikPersonel = new AkademikPersonel
            {
                Ad = akademikPersonelRegisterDto.FirstName + " " + akademikPersonelRegisterDto.LastName,
                Unvan = "Unvan-" + akademikPersonelRegisterDto.UnvanId.ToString(), // TODO: Unvan tablosundan çekilecek
                CreatedDate = DateTime.Now,
                Status = true
                // UserId Dal tarafından atanacak
            };
            
            // 3. İŞ KURALI: Role hazırla
            var userOperationClaim = new Core.Entities.Concrete.UserOperationClaim
            {
                OperationClaimId = 2, // "akademik.personel" role'ü
                CreatedDate = DateTime.Now,
                Status = true
                // UserId Dal tarafından atanacak
            };

            // 4. DAL: Transaction içinde kaydet (User → AkademikPersonel → Role)
            try
            {
                _akademikPersonelDal.AddAkademikPersonelWithUserOperationClaimAsync(
                    user, 
                    akademikPersonel, 
                    userOperationClaim
                ).Wait(); // Async metodu senkron çağır (Business layer'da async yok)
                
                return new SuccessDataResult<User>(user, "Akademik personel kaydı başarıyla oluşturuldu");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<User>("Akademik personel kaydı başarısız: " + ex.Message);
            }
        }

        public IDataResult<User> Login(UserForLoginDto userForLoginDto)
        {
            var userToCheck = userForLoginDto.Email != null 
                ? _userService.GetByMail(userForLoginDto.Email)
                : _userService.GetByUserName(userForLoginDto.UserName);

            if (!userToCheck.Success)
            {
                return new ErrorDataResult<User>(userToCheck.Message);
            }

            if (!HashingHelper.VerifyPasswordHash(userForLoginDto.Password, userToCheck.Data.PasswordHash, userToCheck.Data.PasswordSalt))
            {
                return new ErrorDataResult<User>(Messages.PasswordError);
            }

            return new SuccessDataResult<User>(userToCheck.Data, Messages.SuccessfulLogin);
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
            try
            {
                var claims = _userService.GetClaims(user).Data;
                List<Claim> bolumIdleri = new List<Claim>();

                // Öğrenci role kontrolü
                if (claims != null && claims.Any(c => c.Name == "ogrenci"))
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
                else if (claims != null && claims.Any(c => c.Name == "bolum.gorevlisi"))
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
            catch (Exception err)
            {
                return new ErrorDataResult<AccessToken>("Token Oluşturulurken Bir Hata Alındı." + err.Message);
            }
        }
    }
}
