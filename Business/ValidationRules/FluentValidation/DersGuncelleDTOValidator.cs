using Entity.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class DersGuncelleDTOValidator : AbstractValidator<DersGuncelleDTO>
    {
        public DersGuncelleDTOValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Geçerli bir ders ID'si giriniz");

            RuleFor(x => x.Ad)
                .NotEmpty().WithMessage("Ders adı boş olamaz")
                .MinimumLength(2).WithMessage("Ders adı en az 2 karakter olmalıdır")
                .MaximumLength(200).WithMessage("Ders adı en fazla 200 karakter olabilir");

            RuleFor(x => x.Kod)
                .NotEmpty().WithMessage("Ders kodu boş olamaz")
                .MinimumLength(2).WithMessage("Ders kodu en az 2 karakter olmalıdır")
                .MaximumLength(50).WithMessage("Ders kodu en fazla 50 karakter olabilir")
                .Matches("^[A-Z0-9]+$").WithMessage("Ders kodu sadece büyük harf ve rakam içerebilir (örn: MAT101, BLM201)");
        }
    }
}
