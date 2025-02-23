using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.CourseSubChapters;

public class CourseSubChapterDtoValidator : AbstractValidator<CourseSubChapterDto>
{
    public CourseSubChapterDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CourseChapterId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(5).MaximumLength(255);
        RuleFor(x => x.Content).NotEmpty().MinimumLength(5).MaximumLength(2000);
        RuleFor(x => x.EstimateTime).NotEmpty();
        RuleFor(x => x.Number).NotEmpty();
    }
}