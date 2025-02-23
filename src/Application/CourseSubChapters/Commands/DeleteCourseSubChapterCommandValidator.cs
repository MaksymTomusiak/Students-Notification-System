using FluentValidation;

namespace Application.CourseSubChapters.Commands;

public class DeleteCourseSubChapterCommandValidator : AbstractValidator<DeleteCourseSubChapterCommand>
{
    public DeleteCourseSubChapterCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}