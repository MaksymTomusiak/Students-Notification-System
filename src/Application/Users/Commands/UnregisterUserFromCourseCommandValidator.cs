using FluentValidation;

namespace Application.Users.Commands;

public class UnregisterUserFromCourseCommandValidator : AbstractValidator<UnregisterUserFromCourseCommand>
{
    public UnregisterUserFromCourseCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
    }
}