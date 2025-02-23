using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.CourseChapters;

public class CourseChapterUpdateDtoValidator : AbstractValidator<CourseChapterUpdateDto>
{
    public CourseChapterUpdateDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(5).MaximumLength(255);
        RuleFor(x => x.EstimatedTime).NotEmpty();
    }
}