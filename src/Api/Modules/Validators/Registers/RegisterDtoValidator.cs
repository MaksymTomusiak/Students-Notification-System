using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.Registers;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.RegisteredAt).NotEmpty();
    }
}