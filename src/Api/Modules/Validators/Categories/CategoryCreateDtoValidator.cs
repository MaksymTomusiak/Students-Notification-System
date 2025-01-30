using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.Categories;

public class CategoryCreateDtoValidator : AbstractValidator<CategoryCreateDto>
{
    public CategoryCreateDtoValidator()
    {
        RuleFor(x => x.Name).MinimumLength(3).MaximumLength(255);
    }
}