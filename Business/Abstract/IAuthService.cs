using Core.Entities.Concrete;
using Core.Utilities.Results;
using Core.Utilities.Security.JWT;
using Entity.DTOs;

namespace Business.Abstract
{
    public interface IAuthService
    {
        IDataResult<User> Register(UserForRegisterDto userForRegisterDto, string password);
        Task<IDataResult<CreatedAccountDto>> RegisterOgrenciAsync(OgrenciRegisterDto ogrenciRegisterDto);
        Task<IDataResult<User>> RegisterAkademikPersonelAsync(AkademikPersonelRegisterDto akademikPersonelRegisterDto);
        IDataResult<User> Login(UserForLoginDto userForLoginDto);
        IResult UserExists(string email);
        IDataResult<AccessToken> CreateAccessToken(User user);

        /// <summary>Mevcut parolayı doğrular, yeni parolayı kaydeder ve "parola değiştirilmeli" işaretini kaldırır.</summary>
        IDataResult<User> ChangePassword(int userId, ChangePasswordDto dto);

        /// <summary>
        /// Yönetici için: geçici parola üretir ve kullanıcıyı ilk girişte parola değiştirmeye zorlar.
        /// super.admin hesabının parolasını yalnızca başka bir super.admin sıfırlayabilir.
        /// </summary>
        IDataResult<CreatedAccountDto> ResetPassword(int targetUserId, bool actorIsSuperAdmin);
    }
}
