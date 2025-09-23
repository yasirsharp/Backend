using Entity.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class AkademikPersonelValidator : AbstractValidator<AkademikPersonel>
    {
        public AkademikPersonelValidator()
        {
            RuleFor(x => x.Ad).NotEmpty().WithMessage("Ad boş olamaz");
            RuleFor(x => x.Unvan).NotEmpty().WithMessage("Unvan boş olamaz");
        }
    }
}
