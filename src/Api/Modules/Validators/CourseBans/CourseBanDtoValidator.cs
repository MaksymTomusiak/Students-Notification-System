using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.CourseBans;

public class CourseBanDtoValidator : AbstractValidator<CourseBanDto>
{
    public CourseBanDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.BannedAt).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty()
            .MinimumLength(5)
            .MaximumLength(255);
    }
}