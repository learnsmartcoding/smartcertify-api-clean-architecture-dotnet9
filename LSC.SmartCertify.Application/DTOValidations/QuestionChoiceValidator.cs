using FluentValidation;
using LSC.SmartCertify.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSC.SmartCertify.Application.DTOValidations
{
    public class QChoiceValidator : AbstractValidator<ChoiceDto>
    {
        public QChoiceValidator()
        {
            RuleFor(x => x.ChoiceText).NotEmpty().WithMessage("Choice text is required.");
        }
    }

    public class CQuestionValidator : AbstractValidator<QuestionDto>
    {
        public CQuestionValidator()
        {
            RuleFor(x => x.QuestionText).NotEmpty().WithMessage("Question text is required.");
            RuleFor(x => x.DifficultyLevel).NotEmpty().WithMessage("Difficulty level is required.");
        }
    }
}
