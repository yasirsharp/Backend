using Entity.DTOs;
using FluentValidation;

namespace Business.ValidationRules.FluentValidation
{
    public class OgrenciCreateDtoValidator : AbstractValidator<OgrenciCreateDto>
    {
        public OgrenciCreateDtoValidator()
        {
            RuleFor(x => x.OgrenciNo).NotEmpty().WithMessage("Öğrenci numarası boş olamaz.")
                .MaximumLength(20).WithMessage("Öğrenci numarası en fazla 20 karakter olabilir.")
                .Matches("^[A-Za-z0-9]+$").WithMessage("Öğrenci numarası sadece harf ve rakam içerebilir.");
            RuleFor(x => x.Ad).NotEmpty().WithMessage("Ad boş olamaz.").MaximumLength(100);
            RuleFor(x => x.Soyad).NotEmpty().WithMessage("Soyad boş olamaz.").MaximumLength(100);
            RuleFor(x => x.Email).EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
                .MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Email));
            RuleFor(x => x.BolumId).GreaterThan(0).WithMessage("Geçerli bir bölüm seçiniz.");
            RuleFor(x => x.Sinif).InclusiveBetween(1, 8).When(x => x.Sinif.HasValue)
                .WithMessage("Sınıf 1 ile 8 arasında olmalıdır.");
        }
    }

    public class OgrenciUpdateDtoValidator : AbstractValidator<OgrenciUpdateDto>
    {
        public OgrenciUpdateDtoValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.OgrenciNo).NotEmpty().WithMessage("Öğrenci numarası boş olamaz.").MaximumLength(20);
            RuleFor(x => x.Ad).NotEmpty().WithMessage("Ad boş olamaz.").MaximumLength(100);
            RuleFor(x => x.Soyad).NotEmpty().WithMessage("Soyad boş olamaz.").MaximumLength(100);
            RuleFor(x => x.BolumId).GreaterThan(0).WithMessage("Geçerli bir bölüm seçiniz.");
            RuleFor(x => x.Sinif).InclusiveBetween(1, 8).When(x => x.Sinif.HasValue)
                .WithMessage("Sınıf 1 ile 8 arasında olmalıdır.");
        }
    }

    public class AkademikPersonelCreateDtoValidator : AbstractValidator<AkademikPersonelCreateDto>
    {
        public AkademikPersonelCreateDtoValidator()
        {
            RuleFor(x => x.Ad).NotEmpty().WithMessage("Ad soyad boş olamaz.").MaximumLength(200);
            RuleFor(x => x.Unvan).NotEmpty().WithMessage("Unvan boş olamaz.").MaximumLength(100);
            RuleFor(x => x.Email).EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
                .MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Email));
        }
    }

    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Mevcut parolanızı giriniz.");
            RuleFor(x => x.NewPassword).StrongPassword()
                .NotEqual(x => x.CurrentPassword).WithMessage("Yeni parola mevcut paroladan farklı olmalıdır.");
        }
    }
}
