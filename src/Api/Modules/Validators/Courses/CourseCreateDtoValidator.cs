using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.Courses;

public class CourseCreateDtoValidator : AbstractValidator<CourseCreateDto>
{
    public CourseCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(5).MaximumLength(255);
        RuleFor(x => x.Description).NotEmpty().MinimumLength(5).MaximumLength(1000);
        RuleFor(x => x.ImageUrl).NotEmpty().MinimumLength(5).MaximumLength(1000);
        RuleFor(x => x.StartDate).NotEmpty().GreaterThanOrEqualTo(DateTime.UtcNow);
        RuleFor(x => x.FinishDate).NotEmpty().GreaterThan(x => x.StartDate);
        RuleFor(x => x.CreatorId).NotEmpty();
    }
}