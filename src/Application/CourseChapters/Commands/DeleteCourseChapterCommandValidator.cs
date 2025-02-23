using FluentValidation;

namespace Application.CourseChapters.Commands;

public class DeleteCourseChapterCommandValidator : AbstractValidator<DeleteCourseChapterCommand>
{
    public DeleteCourseChapterCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}