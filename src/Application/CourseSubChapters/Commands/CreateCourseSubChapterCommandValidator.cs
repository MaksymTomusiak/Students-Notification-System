using FluentValidation;

namespace Application.CourseSubChapters.Commands;

public class CreateCourseSubChapterCommandValidator : AbstractValidator<CreateCourseSubChapterCommand>
{
    public CreateCourseSubChapterCommandValidator()
    {
        RuleFor(x => x.CourseChapterId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(5).MaximumLength(255);
        RuleFor(x => x.Content).NotEmpty().MinimumLength(5).MaximumLength(2000);
        RuleFor(x => x.EstimatedLearningTimeMinutes).NotEmpty();
    }
}