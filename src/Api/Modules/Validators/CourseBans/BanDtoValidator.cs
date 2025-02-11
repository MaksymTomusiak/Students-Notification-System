using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.CourseBans;

public class BanDtoValidator : AbstractValidator<BanDto>
{
    public BanDtoValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty()
            .MinimumLength(5)
            .MaximumLength(255);
    }
}