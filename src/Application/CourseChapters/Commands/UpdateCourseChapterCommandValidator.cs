using FluentValidation;

namespace Application.CourseChapters.Commands;

public class UpdateCourseChapterCommandValidator : AbstractValidator<UpdateCourseChapterCommand>
{
    public UpdateCourseChapterCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(5).MaximumLength(255);
        RuleFor(x => x.EstimatedLearningTimeMinutes).NotEmpty();
    }
}