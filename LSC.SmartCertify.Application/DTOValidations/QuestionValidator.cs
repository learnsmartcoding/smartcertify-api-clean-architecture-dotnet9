using FluentValidation;
using LSC.SmartCertify.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSC.SmartCertify.Application.DTOValidations
{
    public class QuestionValidator : AbstractValidator<CreateQuestionDto>
    {
        public QuestionValidator()
        {
            RuleFor(q => q.QuestionText)
                .NotEmpty()
                .WithMessage("Question text is required.")
                .MaximumLength(500)
                .WithMessage("Question text cannot exceed 500 characters.");

            RuleFor(q => q.DifficultyLevel)
                .NotEmpty()
                .WithMessage("Difficulty level is required.")
                .MaximumLength(20)
                .WithMessage("Difficulty level cannot exceed 20 characters.");

            RuleFor(q => q.CourseId)
                .GreaterThan(0)
                .WithMessage("CourseId must be greater than 0.");
        }
    }

    public class UpdateQuestionValidator : AbstractValidator<UpdateQuestionDto>
    {
        public UpdateQuestionValidator()
        {
            RuleFor(q => q.QuestionText)
                .NotEmpty()
                .WithMessage("Question text is required.")
                .MaximumLength(500)
                .WithMessage("Question text cannot exceed 500 characters.");

            RuleFor(q => q.DifficultyLevel)
                .NotEmpty()
                .WithMessage("Difficulty level is required.")
                .MaximumLength(20)
                .WithMessage("Difficulty level cannot exceed 20 characters.");
        }
    }

}
