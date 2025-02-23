using FluentValidation;

namespace Application.CourseChapters.Commands;

public class CreateCourseChapterCommandValidator : AbstractValidator<CreateCourseChapterCommand>
{
    public CreateCourseChapterCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(5).MaximumLength(255);
        RuleFor(x => x.EstimatedLearningTimeMinutes).NotEmpty();
    }
}