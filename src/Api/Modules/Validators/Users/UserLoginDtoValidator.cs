using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.Users;

public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
{
    public UserLoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(255);
    }
}