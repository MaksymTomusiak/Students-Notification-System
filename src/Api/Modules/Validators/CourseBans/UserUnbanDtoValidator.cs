using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.CourseBans;

public class UserUnbanDtoValidator : AbstractValidator<UnbanDto>
{
    public UserUnbanDtoValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
    }
}