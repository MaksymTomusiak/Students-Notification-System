using FluentValidation;

namespace Application.CourseBans.Commands;

public class UnbanUserFromCourseCommandValidator : AbstractValidator<UnbanUserFromCourseCommand>
{
    public UnbanUserFromCourseCommandValidator()
    {
        RuleFor(x => x.BanId).NotEmpty();
    }
}