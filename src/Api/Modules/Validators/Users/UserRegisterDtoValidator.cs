using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.Users;

public class UserRegisterDtoValidator : AbstractValidator<UserRegisterDto>
{
    public UserRegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);
        RuleFor(x => x.Password)
            .NotEmpty()
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$")
            .MaximumLength(255);
        RuleFor(x => x.UserName)
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(255);
    }
}