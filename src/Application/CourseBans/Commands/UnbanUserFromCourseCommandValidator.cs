using FluentValidation;

namespace Application.CourseBans.Commands;

public class UnbanUserFromCourseCommandValidator : AbstractValidator<UnbanUserFromCourseCommand>
{
    public UnbanUserFromCourseCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
    }
}