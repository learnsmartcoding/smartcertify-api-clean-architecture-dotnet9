using FluentValidation;
using LSC.SmartCertify.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSC.SmartCertify.Application.DTOValidations
{
    public class ChoiceValidator : AbstractValidator<CreateChoiceDto>
    {
        public ChoiceValidator()
        {
            RuleFor(c => c.ChoiceText)
                .NotEmpty()
                .WithMessage("Choice text is required.")
                .MaximumLength(200)
                .WithMessage("Choice text cannot exceed 200 characters.");

            RuleFor(c => c.QuestionId)
                .GreaterThan(0)
                .WithMessage("QuestionId must be greater than 0.");
        }
    }

    public class UpdateChoiceValidator : AbstractValidator<UpdateChoiceDto>
    {
        public UpdateChoiceValidator()
        {
            RuleFor(c => c.ChoiceText)
                .NotEmpty()
                .WithMessage("Choice text is required.")
                .MaximumLength(200)
                .WithMessage("Choice text cannot exceed 200 characters.");
        }
    }

}
