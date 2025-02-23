using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.CourseChapters;

public class CourseChapterCreateDtoValidator : AbstractValidator<CourseChapterCreateDto>
{
    public CourseChapterCreateDtoValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(5).MaximumLength(255);
        RuleFor(x => x.EstimatedTime).NotEmpty();
    }
}