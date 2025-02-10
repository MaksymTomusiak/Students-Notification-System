using FluentValidation;

namespace Application.CourseBans.Commands;

public class BanUserFromCourseCommandValidator : AbstractValidator<BanUserFromCourseCommand>
{
    public BanUserFromCourseCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty()
            .MinimumLength(5)
            .MaximumLength(255);
    }
}