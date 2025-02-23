using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.CourseChapters;

public class CourseChaptersUpdateOrderDtoValidator : AbstractValidator<CourseChaptersUpdateOrderDto>
{
    public CourseChaptersUpdateOrderDtoValidator()
    {
        RuleFor(x => x.Numbers).NotEmpty();
        RuleForEach(x => x.ChaptersIds).NotEmpty();
    }
}