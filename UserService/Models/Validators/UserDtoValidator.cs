using FluentValidation;
using UserService.Models.Dtos;

namespace UserService.Models.Validators
{
    public class UserDtoValidator : AbstractValidator<RegisterDto>
    {
        public UserDtoValidator() {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters");



            RuleFor(x => x.AccountNumber)
                .NotEmpty().WithMessage("Account number is required")
                .Matches(@"^\d{10}$").WithMessage("Account number must be exactly 10 digits");
        }
    }
}

