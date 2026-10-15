using Entity.DTOs;
using FluentValidation;

namespace Business.ValidationRules.FluentValidation
{
    internal static class PasswordRules
    {
        public const int MinLength = 8;

        public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule)
        {
            return rule
                .NotEmpty().WithMessage("Parola boş olamaz.")
                .MinimumLength(MinLength).WithMessage($"Parola en az {MinLength} karakter olmalıdır.")
                .MaximumLength(128).WithMessage("Parola en fazla 128 karakter olabilir.")
                .Matches("[A-ZÇĞİÖŞÜ]").WithMessage("Parola en az bir büyük harf içermelidir.")
                .Matches("[a-zçğıöşü]").WithMessage("Parola en az bir küçük harf içermelidir.")
                .Matches("[0-9]").WithMessage("Parola en az bir rakam içermelidir.");
        }
    }

    public class UserForRegisterDtoValidator : AbstractValidator<UserForRegisterDto>
    {
        public UserForRegisterDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");
            RuleFor(x => x.UserName).NotEmpty().MaximumLength(50).WithMessage("Kullanıcı adı boş olamaz.");
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).WithMessage("Ad boş olamaz.");
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).WithMessage("Soyad boş olamaz.");
            RuleFor(x => x.Password).StrongPassword();
        }
    }

    public class OgrenciRegisterDtoValidator : AbstractValidator<OgrenciRegisterDto>
    {
        public OgrenciRegisterDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).WithMessage("Ad boş olamaz.");
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).WithMessage("Soyad boş olamaz.");
            RuleFor(x => x.OgrenciNo).NotEmpty().MaximumLength(20).WithMessage("Öğrenci numarası boş olamaz.");
            RuleFor(x => x.BolumId).GreaterThan(0).WithMessage("Geçerli bir bölüm seçiniz.");
            RuleFor(x => x.Password).StrongPassword();
        }
    }

    public class AkademikPersonelRegisterDtoValidator : AbstractValidator<AkademikPersonelRegisterDto>
    {
        public AkademikPersonelRegisterDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).WithMessage("Ad boş olamaz.");
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).WithMessage("Soyad boş olamaz.");
            RuleFor(x => x.Password).StrongPassword();
        }
    }
}
