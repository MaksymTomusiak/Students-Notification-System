using Api.Dtos;
using FluentValidation;
using Org.BouncyCastle.Math.EC.Multiplier;

namespace Api.Modules.Validators.CourseSubChapters;

public class CourseSubChaptersUpdateOrderDtoValidator : AbstractValidator<CourseSubChaptersUpdateOrderDto>
{
    public CourseSubChaptersUpdateOrderDtoValidator()
    {
        RuleFor(x => x.Numbers).NotEmpty();
        RuleFor(x => x.SubChaptersIds).NotEmpty();
    }
}