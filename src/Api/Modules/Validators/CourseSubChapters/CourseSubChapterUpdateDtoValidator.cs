using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.CourseSubChapters;

public class CourseSubChapterUpdateDtoValidator : AbstractValidator<CourseSubChapterUpdateDto>
{
    public CourseSubChapterUpdateDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(5).MaximumLength(255);
        RuleFor(x => x.Content).NotEmpty().MinimumLength(5).MaximumLength(2000);
        RuleFor(x => x.EstimateTime).NotEmpty();
    }
}