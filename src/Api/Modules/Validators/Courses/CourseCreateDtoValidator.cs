﻿using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.Courses;

public class CourseCreateDtoValidator : AbstractValidator<CourseCreateDto>
{
    public CourseCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(5).MaximumLength(255);
        RuleFor(x => x.Description).NotEmpty().MinimumLength(5).MaximumLength(1000);
        RuleFor(x => x.ImageUrl).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.StartDate.ToUniversalTime()).NotEmpty().GreaterThanOrEqualTo(DateTime.UtcNow - TimeSpan.FromDays(1));
        RuleFor(x => x.FinishDate).NotEmpty().GreaterThan(x => x.StartDate);
        RuleFor(x => x.CreatorId).NotEmpty();
        RuleFor(x => x.Language).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Requirements).NotEmpty().MaximumLength(2000);
    }
}