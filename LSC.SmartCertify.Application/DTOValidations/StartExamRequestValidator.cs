using FluentValidation;
using LSC.SmartCertify.Application.DTOs;

namespace LSC.SmartCertify.Application.DTOValidations
{
    public class StartExamRequestValidator : AbstractValidator<StartExamRequest>
    {
        public StartExamRequestValidator()
        {
            RuleFor(x => x.CourseId).GreaterThan(0).WithMessage("CourseId must be greater than 0.");
            
            RuleFor(x => x.UserId).GreaterThan(0).WithMessage("UserId must be greater than 0.");

            /*
             Pending validations for StartExamRequest.
                1. incoming CourseId should be validated against the existing courses in DB
                2. incoming UserId should be validated against the existing users in DB or take it from IUserClaimsPrincipal
             */
        }
    }

}
