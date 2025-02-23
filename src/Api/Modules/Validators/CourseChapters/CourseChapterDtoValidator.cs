using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.CourseChapters;

public class CourseChapterDtoValidator : AbstractValidator<CourseChapterDto>
{
    public CourseChapterDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(5).MaximumLength(255);
        RuleFor(x => x.EstimatedTime).NotEmpty();
        RuleFor(x => x.Number).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
    }
}