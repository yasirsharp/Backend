using Business.Abstract;
using Business.Constants;
using Core.Entities.Concrete;
using Core.Utilities.Results;
using Core.Utilities.Security.Hashing;
using Core.Utilities.Security.JWT;
using Entity.DTOs;
using System.Security.Claims;

namespace Business.Concrete
{
    public class AuthManager : IAuthService
    {
        private IUserService _userService;
        private ITokenHelper _tokenHelper;

        public AuthManager(IUserService userService,
            ITokenHelper tokenHelper)
        {
            _userService = userService;
            _tokenHelper = tokenHelper;
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
                var userBolums = _userService.GetBolumIds(user.Id).Data;
                List<Claim> bolumIdleri = new List<Claim>();
                if (userBolums.Count<=0 ||userBolums != null)
                {
                    for (int i = 0; i < userBolums.Count; i++)
                    {
                        Claim claim = new Claim($"{i + 1}.BolumId", userBolums[i].ToString());
                        bolumIdleri.Add(claim);
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
