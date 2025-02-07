using FluentValidation;

namespace Application.Users.Commands;

public class EnrollUserInCourseCommandValidator : AbstractValidator<EnrollUserInCourseCommand>
{
    public EnrollUserInCourseCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
    }
}