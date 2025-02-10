using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.CourseBans;

public class UserBanDtoValidator : AbstractValidator<BanDto>
{
    public UserBanDtoValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty()
            .MinimumLength(5)
            .MaximumLength(255);
    }
}