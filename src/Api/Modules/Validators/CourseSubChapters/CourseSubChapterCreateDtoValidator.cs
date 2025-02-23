using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.CourseSubChapters;

public class CourseSubChapterCreateDtoValidator : AbstractValidator<CourseSubChapterCreateDto>
{
    public CourseSubChapterCreateDtoValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(5).MaximumLength(255);
        RuleFor(x => x.Content).NotEmpty().MinimumLength(5).MaximumLength(2000);
        RuleFor(x => x.EstimateTime).NotEmpty();
    }
}